//
// hub-models.ts
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// TypeScript interfaces for VolunteerHub API responses.
// Replaces ad-hoc `any` types across hub components and services.
//

export interface VolunteerProfile {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    status: string;
    totalHoursServed: number;
    memberSince?: string;
    availabilityPreferences?: string;
    interestsAndSkillsNotes?: string;
    emergencyContactNotes?: string;
    profilePhotoUrl?: string;
    backgroundCheckDate?: string;
    onboardedDate?: string;
    resource?: { name: string };
    volunteerStatus?: { name: string };
}


export interface VolunteerAssignment {
    id: number;
    eventName: string;
    eventDescription?: string;
    startDateTime: string;
    endDateTime: string;
    role?: string;
    status: string;
    reportedHours?: number;
    approvedHours?: number;
    notes?: string;
    location?: string;
}


export interface Opportunity {
    id: number;
    name: string;
    description?: string;
    location?: string;
    startDateTime: string;
    endDateTime: string;
    totalVolunteerSlots?: number;
    currentVolunteers: number;
    isAlreadySignedUp: boolean;
}


export interface BrandingInfo {
    organizationName: string;
    email?: string;
    phoneNumber?: string;
    website?: string;
    primaryColor?: string;
    secondaryColor?: string;
    hasLogo: boolean;
    logoUrl?: string;
}


export interface OtpRequestResult {
    message: string;
    status?: string;  // 'pending' if volunteer has a pending registration
}


export interface ProfileUpdateRequest {
    availabilityPreferences: string;
    interestsAndSkillsNotes: string;
    emergencyContactNotes: string;
}
