import React, {lazy, Suspense} from 'react';

const LicWinLostReport = lazy(()=> import('./pages/Report/LicenseeWinLost/index'));
const GroupAuthoritySetting = lazy(()=> import('./pages/Administration/GroupAuthoritySetting/index'));
const UserAuthoritySetting = lazy(()=> import('./pages/Administration/UserAuthoritySetting/index'));
const PaymentAdjustmentReport = lazy(()=> import('./pages/Finance/PaymentAdjustmentReport/index'));
const BonusInfoReport = lazy(()=> import('./pages/Marketing/BonusInfoReport/index'));
const PaymentManagement = lazy(()=> import('./pages/Finance/PaymentManagement/index'));

export const defaultPage = 'PaymentManagement';
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
  'PaymentAdjustmentReport':{
    comp: <Suspense fallback="loading..."><PaymentAdjustmentReport /></Suspense>,
    css:['./lib/bootstrap/dist/css/bootstrap.min.css', './css/main.css', './lib/Oneworks/jscalendar/calendar-blue.css'],
    layout:layoutType.N88
  },
  'BonusReport':{
    comp: <Suspense fallback="loading..."><BonusInfoReport /></Suspense>,
    css:['./lib/bootstrap/dist/css/bootstrap.min.css', './css/main.css', './lib/Oneworks/jscalendar/calendar-blue.css'],
    layout:layoutType.N88
  },
  'PaymentManagement':{
    comp: <Suspense fallback="loading..."><PaymentManagement /></Suspense>,
    css:[
      './lib/bootstrap/dist/css/bootstrap.min.css', 
      // './css/main.css', 
      './lib/Oneworks/jscalendar/calendar-blue.css'],
    layout:layoutType.N88
  }
}