import React from 'react';
import GroupAuthoritySetting from './pages/Administration/GroupAuthoritySetting/index';
import UserAuthoritySetting from './pages/Administration/UserAuthoritySetting/index';
import LicWinLostReport from './pages/Report/LicenseeWinLost/index'

export const defaultPage = "LicWinLostReport";

export const pages = {
  'GroupAuthoritySetting':<GroupAuthoritySetting />,
  'UserAuthoritySetting':<UserAuthoritySetting />,
  'LicWinLostReport':<LicWinLostReport />
}