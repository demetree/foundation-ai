
//
// Request model for Admin/CreateUser endpoint.
// Creates a new user with password in a single atomic transaction.
//
export interface AdminCreateUserRequest {
  accountName: string;
  password: string;
  firstName?: string | null;
  middleName?: string | null;
  lastName?: string | null;
  emailAddress?: string | null;
  cellPhoneNumber?: string | null;
  phoneNumber?: string | null;
  phoneExtension?: string | null;
  description?: string | null;
  securityUserTitleId?: number | null;
  reportsToSecurityUserId?: number | null;
  securityTenantId?: number | null;
  securityOrganizationId?: number | null;
  securityDepartmentId?: number | null;
  securityTeamId?: number | null;
  readPermissionLevel?: number;
  writePermissionLevel?: number;
  mustChangePassword?: boolean;
  twoFactorSendByEmail?: boolean;
  twoFactorSendBySMS?: boolean;
}
