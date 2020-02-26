import React from 'react';
import GroupAuthoritySetting from './pages/Administration/GroupAuthoritySetting/index'
import UserAuthoritySetting from './pages/Administration/UserAuthoritySetting/index'

export const defaultPage = "UserAuthoritySetting";

export const pages = {
  'GroupAuthoritySetting':<GroupAuthoritySetting />,
  'UserAuthoritySetting':<UserAuthoritySetting />
}