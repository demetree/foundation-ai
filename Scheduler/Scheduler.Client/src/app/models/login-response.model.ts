export interface LoginResponse {
  id_token: string;
  access_token: string;
  refresh_token: string;
  expires_in: number;
  token_type: string;
  scope: string;
}


export interface IdToken {
  iat: number;
  exp: number;
  iss: string;
  aud: string | string[];
  sub: string;
  role: string | string[];
  name: string;
  email: string;
  phone_number: string;
  full_name: string;
  settings: string;
  read_permission: string;
  write_permission: string;
  tenant_name: string;
}
