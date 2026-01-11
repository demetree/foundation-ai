export class User {
  constructor(public id = '',
              public userName = '',
              public fullName = '',
              public email = '',
              public settings = '',
              public readPermission = 0,
              public writePermission = 0,
              public tenantName = '',
              roles: string[] = []) {

    if (roles && roles.length > 0 && roles[0] != undefined) {
      this.roles = roles;
    } else {
      this.roles = new Array<string>();
    }
  }
  public roles: string[] = [];
}
