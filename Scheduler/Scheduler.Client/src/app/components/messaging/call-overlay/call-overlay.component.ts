/*

   CALL OVERLAY COMPONENT
   =======================================================================================
   Root-level overlay component that handles 3 call states:
     1. Incoming call modal — caller info, accept/decline buttons
     2. Active call view — duration timer, mute/video toggle, end call
     3. Minimized call bar — small floating bar during navigation

   Renders at the app root level (not inside chat) so calls persist across navigation.

   AI-developed as part of Foundation.Messaging Phase 4 (Calling), March 2026.

*/
import { Component, OnInit, OnDestroy, AfterViewChecked, ViewChild, ElementRef } from '@angular/core';
import { Subscription } from 'rxjs';
import { CallService, CallState, ActiveCall } from '../../../services/call.service';


@Component({
  selector: 'app-call-overlay',
  templateUrl: './call-overlay.component.html',
  styleUrls: ['./call-overlay.component.scss']
})
export class CallOverlayComponent implements OnInit, OnDestroy, AfterViewChecked {

  @ViewChild('remoteVideo') remoteVideoRef!: ElementRef<HTMLVideoElement>;
  @ViewChild('localVideo') localVideoRef!: ElementRef<HTMLVideoElement>;
  @ViewChild('remoteAudio') remoteAudioRef!: ElementRef<HTMLAudioElement>;

  callState: CallState = 'idle';
  activeCall: ActiveCall | null = null;
  callDuration: number = 0;
  formattedDuration: string = '00:00';
  isMuted: boolean = false;
  isVideoOff: boolean = false;
  isMinimized: boolean = false;
  isRemoteVideoOff: boolean = false;

  private subscriptions: Subscription[] = [];
  private pendingRemoteStream: MediaStream | null = null;
  private pendingLocalStream: MediaStream | null = null;


  constructor(public callService: CallService) {}


  ngOnInit(): void {
    //
    // Subscribe to call state changes
    //
    this.subscriptions.push(
      this.callService.callState$.subscribe(state => {
        this.callState = state;

        //
        // Auto-expand when call becomes active
        //
        if (state === 'active' || state === 'ringing') {
          this.isMinimized = false;
        }

        //
        // Reset when idle
        //
        if (state === 'idle') {
          this.isMinimized = false;
        }
      })
    );


    //
    // Subscribe to active call info
    //
    this.subscriptions.push(
      this.callService.activeCall$.subscribe(call => {
        this.activeCall = call;
      })
    );


    //
    // Subscribe to call duration
    //
    this.subscriptions.push(
      this.callService.callDuration$.subscribe(duration => {
        this.callDuration = duration;
        this.formattedDuration = this.callService.formatDuration(duration);
      })
    );


    //
    // Subscribe to mute/video state
    //
    this.subscriptions.push(
      this.callService.isMuted$.subscribe(muted => this.isMuted = muted)
    );

    this.subscriptions.push(
      this.callService.isVideoOff$.subscribe(off => this.isVideoOff = off)
    );


    //
    // Attach remote stream to video element when available
    //
    this.subscriptions.push(
      this.callService.remoteStream$.subscribe(stream => {

        //
        // Always attach to the audio element for audio playback
        //
        if (this.remoteAudioRef?.nativeElement && stream) {
          this.remoteAudioRef.nativeElement.srcObject = stream;
          this.remoteAudioRef.nativeElement.play().catch(() => {});
        }

        //
        // Attach to video element if available, otherwise queue for AfterViewChecked
        //
        if (stream) {
          if (this.remoteVideoRef?.nativeElement) {
            this.remoteVideoRef.nativeElement.srcObject = stream;
          } else {
            this.pendingRemoteStream = stream;
          }

          //
          // Track remote video track state for camera-off placeholder
          //
          const videoTrack = stream.getVideoTracks()[0];
          if (videoTrack) {
            this.isRemoteVideoOff = !videoTrack.enabled || videoTrack.muted;
            videoTrack.onmute = () => { this.isRemoteVideoOff = true; };
            videoTrack.onunmute = () => { this.isRemoteVideoOff = false; };
            videoTrack.onended = () => { this.isRemoteVideoOff = true; };
          } else {
            this.isRemoteVideoOff = true;
          }
        } else {
          this.pendingRemoteStream = null;
          this.isRemoteVideoOff = false;
        }
      })
    );


    //
    // Attach local stream to video element when available
    //
    this.subscriptions.push(
      this.callService.localStream$.subscribe(stream => {
        if (this.localVideoRef?.nativeElement && stream) {
          this.localVideoRef.nativeElement.srcObject = stream;
        } else if (stream) {
          this.pendingLocalStream = stream;
        }
      })
    );
  }


  ngAfterViewChecked(): void {
    //
    // Retry-attach streams to video elements that may not have existed
    // when the stream first arrived (due to *ngIf rendering timing)
    //
    if (this.pendingRemoteStream && this.remoteVideoRef?.nativeElement) {
      this.remoteVideoRef.nativeElement.srcObject = this.pendingRemoteStream;
      this.pendingRemoteStream = null;
    }

    if (this.pendingLocalStream && this.localVideoRef?.nativeElement) {
      this.localVideoRef.nativeElement.srcObject = this.pendingLocalStream;
      this.pendingLocalStream = null;
    }
  }


  ngOnDestroy(): void {
    this.subscriptions.forEach(s => s.unsubscribe());
  }


  // ─── UI Actions ──────────────────────────────────────────────────────────

  acceptCall(): void {
    this.callService.acceptIncomingCall();
  }

  declineCall(): void {
    this.callService.declineIncomingCall();
  }

  endCall(): void {
    this.callService.endCall();
  }

  toggleMute(): void {
    this.callService.toggleMute();
  }

  toggleVideo(): void {
    this.callService.toggleVideo();
  }

  minimize(): void {
    this.isMinimized = true;
  }

  expand(): void {
    this.isMinimized = false;
  }

  get isVideoCall(): boolean {
    return this.activeCall?.callType === 'Video';
  }

  get isIncoming(): boolean {
    return this.activeCall?.direction === 'incoming';
  }

  get callTypeIcon(): string {
    return this.isVideoCall ? 'fa-video' : 'fa-phone';
  }
}
