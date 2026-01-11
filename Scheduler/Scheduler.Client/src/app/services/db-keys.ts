import { Injectable } from '@angular/core';

@Injectable()
export class DBkeys {
  public static readonly CURRENT_USER = 'current_user';
  public static readonly USER_ROLES = 'user_roles';
  public static readonly ACCESS_TOKEN = 'access_token';
  public static readonly REFRESH_TOKEN = 'refresh_token';
  public static readonly TOKEN_EXPIRES_IN = 'expires_in';

  public static readonly TWITTER_OAUTH_TOKEN = 'twitter_oauth_token';
  public static readonly TWITTER_OAUTH_TOKEN_SECRET = 'twitter_oauth_token_secret';

  public static readonly OIDC_TEMP_STORAGE = 'oidc_temp_storage';

  public static readonly REMEMBER_ME = 'remember_me';

  public static readonly LANGUAGE = 'language';
  public static readonly GLOBAL_LANGUAGE = 'global_language';
  public static readonly HOME_URL = 'home_url';
  public static readonly THEME_ID = 'themeId';

  public static readonly USER_CONFIG_KEYS = 'user_config_keys';
}
