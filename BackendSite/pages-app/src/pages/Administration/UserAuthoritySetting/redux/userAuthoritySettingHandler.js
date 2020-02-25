import {reducerActions, reducerPropDefines} from './actionsAndDataProperties'

const ActionTypes = reducerActions.UserAuthoritySettingActionTypes

export function UserAuthoritySettingHandler(state, action){
    switch(action.type){
        case ActionTypes.GET_USER_LIST:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        UserList:action.datas
                    }
                })
        case ActionTypes.CLICK_ENABLE_USER:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        UserList:action.UserList
                    }
                })
        case ActionTypes.BATCH_CLICK_MODIFY_CURRENCY:
                return Object.assign({}, state, {
                    ...state,
                    BatchInfo:{
                        ...state.BatchInfo,
                        [reducerPropDefines.BatchInfo.CurrencyList]:action.CurrencyList,
                        [reducerPropDefines.BatchInfo.IsModify]:action.IsModify
                    }
                })
        case ActionTypes.BATCH_CLOSE_MODIFY_CURRENCY:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        UserList:action.UserList
                    },
                    BatchInfo:{
                        ...state.BatchInfo,
                        [reducerPropDefines.BatchInfo.SelectedCurrency]:null,
                        [reducerPropDefines.BatchInfo.CurrencyList]:[],
                        [reducerPropDefines.BatchInfo.IsModify]:false,
                        [reducerPropDefines.BatchInfo.IsSelectAll]:false
                    }
                })
        case ActionTypes.BATCH_SELECT_CURRENCY:
                return Object.assign({}, state, {
                    ...state,
                    BatchInfo:{
                        ...state.BatchInfo,
                        [reducerPropDefines.BatchInfo.SelectedCurrency]:action.datas,
                        [reducerPropDefines.BatchInfo.IsSelectAll]:action.IsSelectAll
                    },
                    Datas:{
                        ...state.Datas,
                        UserList:action.UserList
                    }
                })
        case ActionTypes.BATCH_SELECT_USER:
                return Object.assign({}, state, {
                    ...state,
                    Datas:{
                        ...state.Datas,
                        UserList:action.UserList
                    },
                    BatchInfo:{
                        ...state.BatchInfo,
                        [reducerPropDefines.BatchInfo.IsSelectAll]:action.IsSelectAll
                    }
                })
        case ActionTypes.BATCH_SELECT_ALL_USER:
                return Object.assign({}, state, {
                    ...state,
                    Datas:{
                        ...state.Datas,
                        UserList:action.UserList
                    },
                    BatchInfo:{
                        ...state.BatchInfo,
                        [reducerPropDefines.BatchInfo.IsSelectAll]:action.IsSelectAll
                    }
                })
        case ActionTypes.BATCH_CLICK_ADD:
                return Object.assign({}, state, {
                    ...state,
                    BatchInfo:{
                        ...state.BatchInfo,
                        [reducerPropDefines.BatchInfo.UpdateUserList]:action.UpdateUserList,
                        [reducerPropDefines.BatchInfo.SelectedCurrencyId]:action.CurrencyID,
                        [reducerPropDefines.BatchInfo.Action]:action.Action,
                        [reducerPropDefines.BatchInfo.ContentTitle]:action.ContentTitle
                    },
                    PopboxInfo: {
                        ...state.PopboxInfo,
                        PopboxName:action.PopboxName,
                        Title:action.Title,
                        ExecutingStatus:null,
                        IsOpen:true
                    }
                })
        case ActionTypes.BATCH_CLICK_REMOVE:
                return Object.assign({}, state, {
                    ...state,
                    BatchInfo:{
                        ...state.BatchInfo,
                        [reducerPropDefines.BatchInfo.UpdateUserList]:action.UpdateUserList,
                        [reducerPropDefines.BatchInfo.SelectedCurrencyId]:action.CurrencyID,
                        [reducerPropDefines.BatchInfo.Action]:action.Action,
                        [reducerPropDefines.BatchInfo.ContentTitle]:action.ContentTitle
                    },
                    PopboxInfo: {
                        ...state.PopboxInfo,
                        PopboxName:action.PopboxName,
                        Title:action.Title,
                        ExecutingStatus:null,
                        IsOpen:true
                    }
                })
        case ActionTypes.BATCH_CLOSE_POPUP_UPDATE_LIST:
                return Object.assign({}, state, {
                    ...state,
                    BatchInfo:{
                        ...state.BatchInfo,
                        [reducerPropDefines.BatchInfo.UpdateUserList]:[],
                        [reducerPropDefines.BatchInfo.SelectedCurrencyId]:null,
                        [reducerPropDefines.BatchInfo.Action]:null,
                        [reducerPropDefines.BatchInfo.ContentTitle]:null
                    },
                    PopboxInfo: {
                        PopboxName:null,
                        Title:null,
                        Content:null,
                        ExecutingStatus:null,
                        IsOpen:false,
                        IdentityKey:null
                    }
                })
        case ActionTypes.BATCH_CLICK_SUBMIT:
                return Object.assign({}, state, {
                    ...state,
                    BatchInfo:{
                        ...state.BatchInfo,
                        [reducerPropDefines.BatchInfo.UpdateUserList]:[],
                        [reducerPropDefines.BatchInfo.SelectedCurrencyId]:null,
                        [reducerPropDefines.BatchInfo.Action]:null,
                        [reducerPropDefines.BatchInfo.ContentTitle]:null
                    },
                    PopboxInfo: {
                        PopboxName:null,
                        Title:null,
                        Content:null,
                        ExecutingStatus:null,
                        IsOpen:false,
                        IdentityKey:null
                    }
                })
        case ActionTypes.CLICK_DELETE_USER:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        UserList:action.UserList
                    }
                })
        default:
            return state
    }
}