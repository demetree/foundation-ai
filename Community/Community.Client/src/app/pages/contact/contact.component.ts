import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PublicContentService } from '../../services/public-content.service';
import { SeoService } from '../../services/seo.service';


@Component({
  selector: 'app-contact',
  template: `
    <div class="page-enter">
      <!-- Header -->
      <div class="contact-hero">
        <div class="container-site">
          <h1>Contact Us</h1>
          <p>Have a question, suggestion, or need more information? We'd love to hear from you.</p>
        </div>
      </div>

      <div class="container-site section">
        <div class="contact-grid">

          <!-- Form -->
          <div class="contact-form-card card-community">
            <div class="card-community__body">
              <h3>Send a Message</h3>

              <form [formGroup]="contactForm" (ngSubmit)="onSubmit()" *ngIf="!submitted">
                <div class="form-group">
                  <label for="name">Your Name *</label>
                  <input id="name" type="text" formControlName="name" class="form-control" placeholder="Jane Doe">
                </div>

                <div class="form-group">
                  <label for="email">Email Address *</label>
                  <input id="email" type="email" formControlName="email" class="form-control" placeholder="jane@example.com">
                </div>

                <div class="form-group">
                  <label for="subject">Subject</label>
                  <input id="subject" type="text" formControlName="subject" class="form-control" placeholder="General Inquiry">
                </div>

                <div class="form-group">
                  <label for="message">Message *</label>
                  <textarea id="message" formControlName="message" class="form-control" rows="5"
                            placeholder="How can we help you?"></textarea>
                </div>

                <button type="submit" class="btn-accent" [disabled]="contactForm.invalid || submitting">
                  <i class="bi bi-send"></i>
                  {{ submitting ? 'Sending...' : 'Send Message' }}
                </button>
              </form>

              <!-- Success Message -->
              <div class="success-message" *ngIf="submitted">
                <i class="bi bi-check-circle-fill"></i>
                <h3>Message Sent!</h3>
                <p>{{ responseMessage }}</p>
                <button class="btn-outline" (click)="resetForm()">Send Another Message</button>
              </div>
            </div>
          </div>

          <!-- Info Sidebar -->
          <div class="contact-info">
            <div class="info-card card-community">
              <div class="card-community__body">
                <div class="info-item">
                  <i class="bi bi-geo-alt-fill"></i>
                  <div>
                    <strong>Town Office</strong>
                    <p>Petty Harbour-Maddox Cove, NL</p>
                  </div>
                </div>
                <div class="info-item">
                  <i class="bi bi-clock-fill"></i>
                  <div>
                    <strong>Office Hours</strong>
                    <p>Monday – Friday: 9:00 AM – 4:30 PM</p>
                  </div>
                </div>
                <div class="info-item">
                  <i class="bi bi-envelope-fill"></i>
                  <div>
                    <strong>Email</strong>
                    <p>Use the form to reach us</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>
  `,
  styles: [`
    .contact-hero {
      background: linear-gradient(135deg, var(--color-primary), var(--color-secondary));
      padding: calc(var(--header-height) + var(--space-3xl)) 0 var(--space-2xl);
      text-align: center;
      h1 { color: white; }
      p { color: rgba(255, 255, 255, 0.7); font-size: 1.1rem; margin-top: var(--space-sm); max-width: 600px; margin-left: auto; margin-right: auto; }
    }
    .contact-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: var(--space-2xl);
      align-items: start;
    }
    .form-group {
      margin-bottom: var(--space-lg);
      label {
        display: block;
        font-weight: 600;
        font-size: 0.9rem;
        margin-bottom: var(--space-xs);
        color: var(--color-text);
      }
    }
    .form-control {
      width: 100%;
      padding: 0.75rem 1rem;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      font-family: var(--font-body);
      font-size: 0.95rem;
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
      &:focus {
        outline: none;
        border-color: var(--color-accent);
        box-shadow: 0 0 0 3px rgba(46, 196, 182, 0.15);
      }
    }
    textarea.form-control {
      resize: vertical;
    }
    .info-item {
      display: flex;
      gap: var(--space-md);
      padding: var(--space-md) 0;
      border-bottom: 1px solid var(--color-border);
      &:last-child { border-bottom: none; }
      i {
        font-size: 1.25rem;
        color: var(--color-accent);
        flex-shrink: 0;
        padding-top: 2px;
      }
      strong { display: block; margin-bottom: var(--space-xs); }
      p { margin: 0; color: var(--color-text-muted); font-size: 0.9rem; }
    }
    .success-message {
      text-align: center;
      padding: var(--space-2xl);
      i { font-size: 3rem; color: var(--color-accent); display: block; margin-bottom: var(--space-md); }
      h3 { margin-bottom: var(--space-sm); }
      p { color: var(--color-text-muted); margin-bottom: var(--space-xl); }
    }
    @media (max-width: 768px) {
      .contact-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class ContactComponent implements OnInit {

  contactForm!: FormGroup;
  submitting: boolean = false;
  submitted: boolean = false;
  responseMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private contentService: PublicContentService,
    private seoService: SeoService
  ) { }

  ngOnInit(): void {
    this.seoService.setPage('Contact Us');
    this.contactForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      subject: [''],
      message: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.contactForm.invalid) return;

    this.submitting = true;

    this.contentService.submitContactForm(this.contactForm.value).subscribe({
      next: (response) => {
        this.submitted = true;
        this.responseMessage = response.message;
        this.submitting = false;
      },
      error: () => {
        this.submitting = false;
        this.responseMessage = 'Something went wrong. Please try again later.';
      }
    });
  }

  resetForm(): void {
    this.submitted = false;
    this.responseMessage = '';
    this.contactForm.reset();
  }
}
