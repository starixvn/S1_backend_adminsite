import React, {lazy, Suspense} from 'react';

const LicWinLostReport = lazy(()=> import('./pages/Report/LicenseeWinLost/index'));
const GroupAuthoritySetting = lazy(()=> import('./pages/Administration/GroupAuthoritySetting/index'));
const UserAuthoritySetting = lazy(()=> import('./pages/Administration/UserAuthoritySetting/index'));
const PaymentAdjustment = lazy(()=> import('./pages/Finance/PaymentAdjustment/index'));
const BonusInfoReport = lazy(()=> import('./pages/Marketing/BonusInfoReport/index'));

export const defaultPage = 'BonusReport';
export const layoutType = {
  N88:'N88',
  LAP:'LAP'
}

export const pages = {
  'GroupAuthoritySetting':{
    comp:<Suspense fallback="loading..."><GroupAuthoritySetting /></Suspense>,
    css:['./fonts//main-font/style.css', './css/common.css'],
    layout:layoutType.LAP
  },
  'UserAuthoritySetting':{
    comp:<Suspense fallback="loading..."><UserAuthoritySetting /></Suspense>,
    css:['./fonts//main-font/style.css', './css/common.css'],
    layout:layoutType.LAP
  },
  'LicWinLostReport':{
    comp:<Suspense fallback="loading..."><LicWinLostReport /></Suspense>,
    css:['./css/pages/report/LicenseeWinLost.css', './lib/Oneworks/jscalendar/calendar-blue.css'],
    layout:layoutType.N88
  },
  'PaymentAdjustment':{
    comp: <Suspense fallback="loading..."><PaymentAdjustment /></Suspense>,
    css:['./lib/bootstrap/dist/css/bootstrap.min.css', './css/main.css', './lib/Oneworks/jscalendar/calendar-blue.css'],
    layout:layoutType.N88
  },
  'BonusReport':{
    comp: <Suspense fallback="loading..."><BonusInfoReport /></Suspense>,
    css:['./lib/bootstrap/dist/css/bootstrap.min.css', './css/main.css', './lib/Oneworks/jscalendar/calendar-blue.css'],
    layout:layoutType.N88
  }
}