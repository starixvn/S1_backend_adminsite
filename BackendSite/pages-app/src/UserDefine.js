import React from 'react';
import GroupAuthoritySetting from './pages/Administration/GroupAuthoritySetting/index'
import UserAuthoritySetting from './pages/Administration/UserAuthoritySetting/index'

export const defaultPage = "GroupAuthoritySetting";

export const pages = {
  'GroupAuthoritySetting':<GroupAuthoritySetting />,
  'UserAuthoritySetting':<UserAuthoritySetting />
}