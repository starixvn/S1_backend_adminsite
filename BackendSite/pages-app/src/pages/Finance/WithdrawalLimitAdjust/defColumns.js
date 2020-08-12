export const adjustType = {
    1:'Debit',
    2:'Credit'
}

export const StatusList = {
    1:'Panding',
    2:'Approved',
    3:'Rejected'
}

export const adjustTypeOpt = Object.keys(adjustType).map((key)=>{return {displayText:adjustType[key], value:key}});

export const FilterColumns = {
    siteName:{ dataColumns:'SiteName' },
    userName:{ dataColumns:'UserName' },
    sabaId:{ dataColumns:'SabaId' },
}

export const SettingColumns = {
    actionType:{dataColumns:'actionType', displayText:'ActionType'},
    amount:{dataColumns:'amount', displayText:'Amount'},
    note:{dataColumns:'note', displayText:'Note'}
}