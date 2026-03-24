/*

   CALL SERVICE
   =======================================================================================
   Central client-side call manager — owns the WebRTC RTCPeerConnection lifecycle,
   media streams, and call state machine.

   State machine:
     Idle → Initiating → Ringing → Connecting → Active → Ending → Idle

   Uses MessagingSignalRService for signaling (SDP offers/answers, ICE candidates)
   and MessagingApiService for REST operations (initiate, accept, decline, end).

   AI-developed as part of Foundation.Messaging Phase 4 (Calling), March 2026.

*/
import { Inject, Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subject, Subscription, interval, firstValueFrom } from 'rxjs';
import { MessagingSignalRService, CallOfferPayload, CallAnswerPayload, CallEndPayload, IceCandidatePayload, SdpPayload } from './messaging-signalr.service';
import { MessagingApiService, CallSummary, CallConnectionInfo, IceServerConfig } from './messaging-api.service';
import { AuthService } from './auth.service';


// ─── Call State ─────────────────────────────────────────────────────────────────

export type CallState = 'idle' | 'initiating' | 'ringing' | 'connecting' | 'active' | 'ending';

export type CallDirection = 'outgoing' | 'incoming';

export interface ActiveCall {
  callId: number;
  conversationId: number;
  callType: string;
  direction: CallDirection;
  localUserId: number;
  remoteUserId: number;
  remoteUserDisplayName: string;
  startTime: Date;
  answerTime: Date | null;
}


@Injectable({
  providedIn: 'root'
})
export class CallService implements OnDestroy {

  // ─── State Observables ──────────────────────────────────────────────────────

  /** Current call state */
  public callState$ = new BehaviorSubject<CallState>('idle');

  /** Active call details (null when idle) */
  public activeCall$ = new BehaviorSubject<ActiveCall | null>(null);

  /** Remote media stream from the other participant */
  public remoteStream$ = new BehaviorSubject<MediaStream | null>(null);

  /** Local media stream (camera/mic) */
  public localStream$ = new BehaviorSubject<MediaStream | null>(null);

  /** Call duration in seconds (ticks while active) */
  public callDuration$ = new BehaviorSubject<number>(0);

  /** Whether local audio is muted */
  public isMuted$ = new BehaviorSubject<boolean>(false);

  /** Whether local video is disabled */
  public isVideoOff$ = new BehaviorSubject<boolean>(false);

  /** Emits errors for the UI to display */
  public callError$ = new Subject<string>();


  // ─── Private State ──────────────────────────────────────────────────────────

  private readonly DEBUG = false;  // Set to true to enable diagnostic logging
  private readonly RING_TIMEOUT_MS = 45000;  // 45 seconds
  private readonly ICE_DISCONNECT_TIMEOUT_MS = 10000;  // 10 seconds

  private peerConnection: RTCPeerConnection | null = null;
  private localStream: MediaStream | null = null;
  private durationTimer: Subscription | null = null;
  private subscriptions: Subscription[] = [];
  private ringtoneAudio: HTMLAudioElement | null = null;
  private ringTimeoutId: any = null;
  private iceDisconnectTimeoutId: any = null;

  //
  // Pending ICE candidates that arrived before the remote description was set
  //
  private pendingIceCandidates: RTCIceCandidateInit[] = [];
  private remoteDescriptionSet = false;
  private pendingSdpOffer: SdpPayload | null = null;
  private callEndedByRemote = false;


  constructor(
    private signalR: MessagingSignalRService,
    private api: MessagingApiService,
    private authService: AuthService
  ) {
    this.setupSignalRSubscriptions();
  }


  ngOnDestroy(): void {
    this.cleanup();
    this.subscriptions.forEach(s => s.unsubscribe());
  }


  // ─── Public API ─────────────────────────────────────────────────────────────

  /**
   * Initiates an outgoing call to a conversation.
   */
  async startCall(conversationId: number, callType: 'Voice' | 'Video'): Promise<void> {
    if (this.callState$.value !== 'idle') {
      this.callError$.next('Already in a call');
      return;
    }

    this.callState$.next('initiating');

    try {
      //
      // 1. Get local media
      //
      const isVideo = callType === 'Video';
      try {
        this.localStream = await navigator.mediaDevices.getUserMedia({
          audio: true,
          video: isVideo
        });
      } catch (mediaError: any) {
        throw new Error(this.getFriendlyMediaErrorMessage(mediaError));
      }
      this.localStream$.next(this.localStream);

      //
      // 2. Initiate call via REST (creates the Call record on the server)
      //
      const callSummary = await firstValueFrom(this.api.initiateCall(conversationId, callType));

      if (!callSummary) {
        throw new Error('Failed to initiate call — no response from server');
      }

      //
      // 3. Set up active call tracking
      //
      const remoteParticipant = callSummary.participants?.find(p => p.role === 'recipient');

      this.activeCall$.next({
        callId: callSummary.id,
        conversationId: conversationId,
        callType: callType,
        direction: 'outgoing',
        localUserId: callSummary.initiatorUserId,
        remoteUserId: remoteParticipant?.userId || 0,
        remoteUserDisplayName: remoteParticipant?.displayName || 'Unknown',
        startTime: new Date(),
        answerTime: null
      });

      //
      // 4. Get ICE server config and create peer connection
      //
      const connectionInfo = await firstValueFrom(this.api.getCallConnectionInfo(callSummary.id));
      await this.createPeerConnection(connectionInfo!);

      //
      // 5. Create and send SDP offer
      //
      const offer = await this.peerConnection!.createOffer();
      await this.peerConnection!.setLocalDescription(offer);

      //
      // 6. Send the SDP offer to all recipients via SignalR
      //
      if (offer.sdp) {
        if (this.DEBUG) console.log(`CallService: Sending SDP offer — local:${callSummary.initiatorUserId} → remote:${remoteParticipant?.userId}`);
        await this.signalR.relaySdpOffer({
          callId: callSummary.id,
          fromUserId: callSummary.initiatorUserId,
          toUserId: remoteParticipant?.userId || 0,
          type: 'offer',
          sdp: offer.sdp
        });
      }

      //
      // We are now ringing — waiting for the remote side to accept
      //
      this.callState$.next('ringing');
      this.playRingtone('outgoing');

      //
      // Start ring timeout — auto-end if no answer within 45 seconds
      //
      this.ringTimeoutId = setTimeout(() => {
        if (this.callState$.value === 'ringing') {
          console.log('CallService: Ring timeout — no answer');
          this.callError$.next('No answer');
          this.endCall();
        }
      }, this.RING_TIMEOUT_MS);

    } catch (error: any) {
      console.error('CallService: Failed to start call', error);
      this.callError$.next(error.message || 'Failed to start call');
      this.cleanup();
    }
  }


  /**
   * Accepts an incoming call.
   */
  async acceptIncomingCall(): Promise<void> {
    const call = this.activeCall$.value;

    if (!call || this.callState$.value !== 'ringing') {
      return;
    }

    this.callState$.next('connecting');
    this.stopRingtone();

    try {
      //
      // 1. Get local media
      //
      const isVideo = call.callType === 'Video';
      this.localStream = await navigator.mediaDevices.getUserMedia({
        audio: true,
        video: isVideo
      });
      this.localStream$.next(this.localStream);

      //
      // 2. Accept the call via REST — response contains our messaging user ID
      //
      const acceptResult = await this.api.acceptCall(call.callId).toPromise();

      //
      // Resolve our local messaging user ID from the accept result
      //
      if (acceptResult?.participants) {
        const myParticipant = acceptResult.participants.find(p => p.role === 'recipient' && p.status === 'joined');
        if (myParticipant) {
          const updated = { ...call, localUserId: myParticipant.userId };
          this.activeCall$.next(updated);
          console.log(`CallService: Answerer local userId resolved: ${myParticipant.userId}`);
        }
      }

      //
      // 3. Get ICE servers and create peer connection
      //
      const connectionInfo = await this.api.getCallConnectionInfo(call.callId).toPromise();
      await this.createPeerConnection(connectionInfo!);

      //
      // 4. If we already received the SDP offer (it arrived before we accepted),
      //    process it now. Otherwise, the sdpOffer$ subscription will handle it.
      //
      if (this.pendingSdpOffer) {
        await this.handleSdpOffer(this.pendingSdpOffer);
        this.pendingSdpOffer = null;
      }

    } catch (error: any) {
      console.error('CallService: Failed to accept call', error);
      this.callError$.next(error.message || 'Failed to accept call');
      this.cleanup();
    }
  }


  /**
   * Declines an incoming call.
   */
  async declineIncomingCall(): Promise<void> {
    const call = this.activeCall$.value;
    if (!call) return;

    this.stopRingtone();

    try {
      await this.signalR.declineCallViaHub(call.callId);
    } catch (error) {
      console.error('CallService: Failed to decline call', error);
    }

    this.cleanup();
  }


  /**
   * Ends the current active call.
   */
  async endCall(): Promise<void> {
    const call = this.activeCall$.value;
    if (!call) return;

    this.callState$.next('ending');

    //
    // Only invoke the server-side EndCall if it wasn't already ended by the remote peer.
    // This prevents a race condition where both sides call EndCall simultaneously,
    // causing the second call to fail with "Call not found" on the server.
    //
    if (!this.callEndedByRemote) {
      try {
        await this.signalR.endCallViaHub(call.callId);
      } catch (error) {
        console.warn('CallService: EndCall hub invocation failed (call likely already ended by remote)', error);
      }
    }

    this.cleanup();
  }


  /**
   * Toggles audio mute on/off.
   */
  toggleMute(): void {
    if (this.localStream) {
      const audioTracks = this.localStream.getAudioTracks();
      audioTracks.forEach(track => {
        track.enabled = !track.enabled;
      });
      this.isMuted$.next(!audioTracks[0]?.enabled);
    }
  }


  /**
   * Toggles video on/off.
   */
  toggleVideo(): void {
    if (this.localStream) {
      const videoTracks = this.localStream.getVideoTracks();
      videoTracks.forEach(track => {
        track.enabled = !track.enabled;
      });
      this.isVideoOff$.next(!videoTracks[0]?.enabled);
    }
  }


  // ─── SignalR Event Handling ────────────────────────────────────────────────

  private setupSignalRSubscriptions(): void {

    //
    // Incoming call — set up as ringing and show the incoming call UI
    //
    this.subscriptions.push(
      this.signalR.incomingCall$.subscribe(payload => {
        if (this.callState$.value !== 'idle') {
          //
          // Already in a call — auto-decline
          //
          this.signalR.declineCallViaHub(payload.callId);
          return;
        }

        this.activeCall$.next({
          callId: payload.callId,
          conversationId: payload.conversationId,
          callType: payload.callType,
          direction: 'incoming',
          localUserId: 0,  // Resolved later when we accept the call
          remoteUserId: payload.initiatorUserId,
          remoteUserDisplayName: payload.initiatorDisplayName,
          startTime: new Date(),
          answerTime: null
        });

        this.callState$.next('ringing');
        this.playRingtone('incoming');
      })
    );


    //
    // Call accepted — the remote side picked up, start connecting
    //
    this.subscriptions.push(
      this.signalR.callAccepted$.subscribe(payload => {
        if (this.activeCall$.value?.callId === payload.callId) {
          this.stopRingtone();
          this.callState$.next('connecting');
        }
      })
    );


    //
    // Call declined — the remote side rejected
    //
    this.subscriptions.push(
      this.signalR.callDeclined$.subscribe(payload => {
        if (this.activeCall$.value?.callId === payload.callId) {
          this.stopRingtone();
          this.callError$.next(`${payload.userDisplayName} declined the call`);
          this.cleanup();
        }
      })
    );


    //
    // Call ended — the remote side hung up
    //
    this.subscriptions.push(
      this.signalR.callEnded$.subscribe(payload => {
        if (this.activeCall$.value?.callId === payload.callId) {
          this.callEndedByRemote = true;
          this.cleanup();
        }
      })
    );


    //
    // ICE candidate from remote peer
    //
    this.subscriptions.push(
      this.signalR.iceCandidate$.subscribe(payload => {
        this.handleRemoteIceCandidate(payload);
      })
    );


    //
    // Missed call notification (informational only)
    //
    this.subscriptions.push(
      this.signalR.callMissed$.subscribe(payload => {
        if (this.activeCall$.value?.callId === payload.callId) {
          this.stopRingtone();
          this.cleanup();
        }
      })
    );


    //
    // SDP offer received from caller — store or process immediately
    //
    this.subscriptions.push(
      this.signalR.sdpOffer$.subscribe(async payload => {
        if (this.activeCall$.value?.callId === payload.callId) {
          if (this.peerConnection) {
            // Peer connection is ready — process the offer now
            await this.handleSdpOffer(payload);
          } else {
            // Peer connection not yet created (user hasn't accepted yet) — queue it
            this.pendingSdpOffer = payload;
          }
        }
      })
    );


    //
    // SDP answer received from answerer (caller-side)
    //
    this.subscriptions.push(
      this.signalR.sdpAnswer$.subscribe(async payload => {
        console.log('CallService: sdpAnswer$ received', {
          payloadCallId: payload.callId,
          activeCallId: this.activeCall$.value?.callId,
          hasPeerConnection: !!this.peerConnection,
          callState: this.callState$.value
        });

        if (this.activeCall$.value?.callId === payload.callId && this.peerConnection) {
          try {
            await this.peerConnection.setRemoteDescription(
              new RTCSessionDescription({ type: 'answer', sdp: payload.sdp })
            );
            this.remoteDescriptionSet = true;
            await this.processPendingIceCandidates();
            console.log('CallService: Remote SDP answer set successfully');
          } catch (error) {
            console.error('CallService: Failed to set remote SDP answer', error);
          }
        } else {
          console.warn('CallService: sdpAnswer$ skipped — callId mismatch or no peer connection');
        }
      })
    );
  }


  // ─── WebRTC Peer Connection ─────────────────────────────────────────────────

  private async createPeerConnection(connectionInfo: CallConnectionInfo): Promise<void> {
    //
    // Map server ICE config to RTCIceServer format
    //
    const rtcConfig: RTCConfiguration = {
      iceServers: connectionInfo.iceServers.map(s => ({
        urls: s.urls,
        username: s.username || undefined,
        credential: s.credential || undefined
      }))
    };

    console.log('CallService: ICE server config:', JSON.stringify(rtcConfig.iceServers?.map(s => ({
      urls: s.urls,
      hasUsername: !!s.username,
      hasCredential: !!s.credential
    })), null, 2));

    this.peerConnection = new RTCPeerConnection(rtcConfig);

    this.remoteDescriptionSet = false;
    // NOTE: Do NOT clear pendingIceCandidates here — they may contain
    // candidates that arrived before the peer connection was created


    //
    // Add local tracks to the connection
    //
    if (this.localStream) {
      this.localStream.getTracks().forEach(track => {
        this.peerConnection!.addTrack(track, this.localStream!);
      });
    }


    //
    // Handle incoming remote tracks
    //
    this.peerConnection.ontrack = (event: RTCTrackEvent) => {
      const remoteStream = event.streams[0];

      if (this.DEBUG) console.log(`CallService: ontrack fired — kind:${event.track.kind} enabled:${event.track.enabled} muted:${event.track.muted} readyState:${event.track.readyState}`);
      if (this.DEBUG) console.log(`CallService: Remote stream tracks: audio=${remoteStream?.getAudioTracks().length} video=${remoteStream?.getVideoTracks().length}`);

      this.remoteStream$.next(remoteStream);

      //
      // When we receive media, the call is now truly active
      //
      if (this.callState$.value === 'connecting') {
        this.callState$.next('active');
        this.startDurationTimer();
        this.clearRingTimeout();
        this.clearIceDisconnectTimeout();

        //
        // Update the answer time
        //
        const call = this.activeCall$.value;
        if (call) {
          call.answerTime = new Date();
          this.activeCall$.next({ ...call });
        }
      }
    };


    //
    // Relay local ICE candidates to the remote peer via SignalR
    //
    this.peerConnection.onicecandidate = (event: RTCPeerConnectionIceEvent) => {
      if (event.candidate) {
        if (this.DEBUG) console.log(`CallService: ICE candidate generated — type:${event.candidate.type} protocol:${event.candidate.protocol} address:${event.candidate.address}`);
        const call = this.activeCall$.value;
        if (call) {
          this.signalR.relayIceCandidate({
            callId: call.callId,
            fromUserId: call.localUserId,
            toUserId: call.remoteUserId,
            candidate: event.candidate.candidate,
            sdpMid: event.candidate.sdpMid || '',
            sdpMLineIndex: event.candidate.sdpMLineIndex || 0
          });
        }
      }
    };


    //
    // Monitor connection state changes
    //
    this.peerConnection.onconnectionstatechange = () => {
      const state = this.peerConnection?.connectionState;
      if (this.DEBUG) console.log('CallService: Connection state:', state);

      if (state === 'failed') {
        this.callError$.next('Connection failed — check your network');
        this.endCall();
      }
    };


    //
    // Monitor ICE connection state — attempt restart on disconnected
    //
    this.peerConnection.oniceconnectionstatechange = () => {
      const state = this.peerConnection?.iceConnectionState;
      if (this.DEBUG) console.log('CallService: ICE connection state:', state);

      if (state === 'disconnected') {
        //
        // Network blip — attempt ICE restart, give it 10 seconds
        //
        console.log('CallService: ICE disconnected — attempting restart...');
        this.peerConnection?.restartIce();

        this.iceDisconnectTimeoutId = setTimeout(() => {
          if (this.peerConnection?.iceConnectionState === 'disconnected' ||
              this.peerConnection?.iceConnectionState === 'failed') {
            console.warn('CallService: ICE restart failed — ending call');
            this.callError$.next('Connection lost — could not reconnect');
            this.endCall();
          }
        }, this.ICE_DISCONNECT_TIMEOUT_MS);

      } else if (state === 'connected' || state === 'completed') {
        this.clearIceDisconnectTimeout();

      } else if (state === 'failed') {
        this.clearIceDisconnectTimeout();
        this.callError$.next('Connection failed — check your network');
        this.endCall();
      }
    };
  }


  private async handleRemoteIceCandidate(payload: IceCandidatePayload): Promise<void> {
    if (this.DEBUG) console.log(`CallService: Remote ICE candidate received — from:${payload.fromUserId} candidate:${payload.candidate?.substring(0, 50)}...`);

    const candidate: RTCIceCandidateInit = {
      candidate: payload.candidate,
      sdpMid: payload.sdpMid,
      sdpMLineIndex: payload.sdpMLineIndex
    };

    if (this.peerConnection && this.remoteDescriptionSet) {
      try {
        await this.peerConnection.addIceCandidate(candidate);
        console.log('CallService: Remote ICE candidate added successfully');
      } catch (error) {
        console.error('CallService: Failed to add ICE candidate', error);
      }
    } else {
      //
      // Queue for later — remote description hasn't been set yet
      //
      console.log(`CallService: Remote ICE candidate queued (peerConnection:${!!this.peerConnection}, remoteDescSet:${this.remoteDescriptionSet})`);
      this.pendingIceCandidates.push(candidate);
    }
  }


  private async processPendingIceCandidates(): Promise<void> {
    if (!this.peerConnection) return;

    console.log(`CallService: Processing ${this.pendingIceCandidates.length} queued ICE candidates`);

    for (const candidate of this.pendingIceCandidates) {
      try {
        await this.peerConnection.addIceCandidate(candidate);
      } catch (error) {
        console.error('CallService: Failed to add queued ICE candidate', error);
      }
    }

    this.pendingIceCandidates = [];
  }


  /**
   * Handles an incoming SDP offer — sets remote description, creates answer, sends it back.
   */
  private async handleSdpOffer(payload: SdpPayload): Promise<void> {
    if (!this.peerConnection) return;

    try {
      //
      // Set the caller's offer as the remote description
      //
      await this.peerConnection.setRemoteDescription(
        new RTCSessionDescription({ type: 'offer', sdp: payload.sdp })
      );
      this.remoteDescriptionSet = true;

      //
      // Process any ICE candidates that arrived before the remote description was set
      //
      await this.processPendingIceCandidates();

      //
      // Create and set the local SDP answer
      //
      const answer = await this.peerConnection.createAnswer();
      await this.peerConnection.setLocalDescription(answer);

      //
      // Send the answer back to the caller via SignalR
      //
      const call = this.activeCall$.value;
      if (call && answer.sdp) {
        console.log(`CallService: Sending SDP answer — local:${call.localUserId} → remote:${call.remoteUserId}`);
        await this.signalR.relaySdpAnswer({
          callId: call.callId,
          fromUserId: call.localUserId,
          toUserId: call.remoteUserId,
          type: 'answer',
          sdp: answer.sdp
        });
      }

      console.log('CallService: SDP offer processed, answer sent');
    } catch (error) {
      console.error('CallService: Failed to handle SDP offer', error);
    }
  }


  /**
   * Sets the remote SDP description (called when receiving offer/answer via SignalR).
   * The current implementation is signaling-only — this will be called from the
   * call overlay component when SDP exchange happens via a custom event or
   * extended signaling payload.
   */
  async setRemoteDescription(sdp: RTCSessionDescriptionInit): Promise<void> {
    if (!this.peerConnection) return;

    await this.peerConnection.setRemoteDescription(sdp);
    this.remoteDescriptionSet = true;
    await this.processPendingIceCandidates();
  }


  // ─── Duration Timer ─────────────────────────────────────────────────────────

  private startDurationTimer(): void {
    this.callDuration$.next(0);

    this.durationTimer = interval(1000).subscribe(() => {
      this.callDuration$.next(this.callDuration$.value + 1);
    });
  }


  // ─── Ringtone ──────────────────────────────────────────────────────────────

  private playRingtone(type: 'incoming' | 'outgoing'): void {
    //
    // Use a simple oscillator via Web Audio API — no external audio files needed.
    // In production, replace with actual ringtone audio files.
    //
    try {
      const context = new AudioContext();
      const oscillator = context.createOscillator();
      const gain = context.createGain();

      oscillator.connect(gain);
      gain.connect(context.destination);

      oscillator.frequency.value = type === 'incoming' ? 440 : 480;
      gain.gain.value = 0.1;

      oscillator.start();

      //
      // Store for cleanup
      //
      (this as any)._ringtoneOscillator = oscillator;
      (this as any)._ringtoneContext = context;
    } catch (error) {
      console.warn('CallService: Could not play ringtone', error);
    }
  }


  private stopRingtone(): void {
    try {
      const oscillator = (this as any)._ringtoneOscillator;
      const context = (this as any)._ringtoneContext;

      if (oscillator) {
        oscillator.stop();
        (this as any)._ringtoneOscillator = null;
      }
      if (context) {
        context.close();
        (this as any)._ringtoneContext = null;
      }
    } catch (error) {
      // Ignore — already stopped
    }
  }


  // ─── Cleanup ───────────────────────────────────────────────────────────────

  private cleanup(): void {
    this.stopRingtone();
    this.clearRingTimeout();
    this.clearIceDisconnectTimeout();

    //
    // Stop duration timer
    //
    if (this.durationTimer) {
      this.durationTimer.unsubscribe();
      this.durationTimer = null;
    }

    //
    // Close peer connection
    //
    if (this.peerConnection) {
      this.peerConnection.close();
      this.peerConnection = null;
    }

    //
    // Stop all local media tracks
    //
    if (this.localStream) {
      this.localStream.getTracks().forEach(track => track.stop());
      this.localStream = null;
    }

    //
    // Reset all state
    //
    this.localStream$.next(null);
    this.remoteStream$.next(null);
    this.activeCall$.next(null);
    this.callDuration$.next(0);
    this.isMuted$.next(false);
    this.isVideoOff$.next(false);
    this.callState$.next('idle');
    this.remoteDescriptionSet = false;
    this.pendingIceCandidates = [];
    this.callEndedByRemote = false;
  }


  private clearRingTimeout(): void {
    if (this.ringTimeoutId) {
      clearTimeout(this.ringTimeoutId);
      this.ringTimeoutId = null;
    }
  }


  private clearIceDisconnectTimeout(): void {
    if (this.iceDisconnectTimeoutId) {
      clearTimeout(this.iceDisconnectTimeoutId);
      this.iceDisconnectTimeoutId = null;
    }
  }


  /**
   * Maps browser getUserMedia error names to user-friendly messages.
   */
  private getFriendlyMediaErrorMessage(error: any): string {
    switch (error.name) {
      case 'NotAllowedError':
        return 'Microphone access denied. Check your browser permissions.';
      case 'NotFoundError':
        return 'No microphone found. Check your device.';
      case 'NotReadableError':
        return 'Microphone is in use by another application.';
      case 'OverconstrainedError':
        return 'Camera or microphone does not meet requirements.';
      case 'AbortError':
        return 'Media access was interrupted. Please try again.';
      default:
        return error.message || 'Failed to access camera/microphone.';
    }
  }


  // ─── Utility ───────────────────────────────────────────────────────────────

  /**
   * Formats a duration in seconds to MM:SS display.
   */
  formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }
}
