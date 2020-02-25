import {reducerActions, reducerPropDefines} from './actionsAndDataProperties'

const ActionTypes = reducerActions.AddUserActionTypes

export function AddUserHandler(state, action){
    switch(action.type){
        case ActionTypes.OPEN_ADD_USER_POPBOX:
                return Object.assign({}, state, {
                    ...state,
                    PopboxInfo: {
                        ...state.PopboxInfo,
                        PopboxName:action.popboxName,
                        Title:action.title,
                        ExecutingStatus:null,
                        IsOpen:true
                    }
                })
        case ActionTypes.UPDATE_CURRENCY_LIST:
                return Object.assign({}, state, {
                    ...state,
                    AddUserPopbox: {
                        ...state.AddUserPopbox,
                        CurrencyList:action.currencyList
                    }
                })
        case ActionTypes.UPDATE_GROUP_LIST:
                return Object.assign({}, state, {
                    ...state,
                    AddUserPopbox: {
                        ...state.AddUserPopbox,
                        GroupList:action.groupList
                    }
                })
        case ActionTypes.CLOSE_ADD_USER_POPBOX:
                return Object.assign({}, state, {
                    ...state,
                    AddUserPopbox:{
                        CurrencyList:[],
                        GroupList:[],
                        AddUserDataModel:{
                            [reducerPropDefines.AddUserDataModelProperty.UserID]: "",
                            [reducerPropDefines.AddUserDataModelProperty.UserName]: "", 
                            [reducerPropDefines.AddUserDataModelProperty.UserPWD]: "",
                            [reducerPropDefines.AddUserDataModelProperty.Email]: "", 
                            [reducerPropDefines.AddUserDataModelProperty.Currency]:[]  
                        }
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
        case ActionTypes.EDIT_FIELD:
                return Object.assign({}, state, {
                    ...state,
                    AddUserPopbox: {
                        ...state.AddUserPopbox,
                        AddUserDataModel:action.datas
                    }
                })
        case ActionTypes.ADD_USER:
                return Object.assign({}, state, {
                    ...state,
                    AddUserPopbox:{
                        CurrencyList:[],
                        GroupList:[],
                        AddUserDataModel:{
                            [reducerPropDefines.AddUserDataModelProperty.UserID]: "",
                            [reducerPropDefines.AddUserDataModelProperty.UserName]: "", 
                            [reducerPropDefines.AddUserDataModelProperty.UserPWD]: "",
                            [reducerPropDefines.AddUserDataModelProperty.Email]: "", 
                            [reducerPropDefines.AddUserDataModelProperty.Currency]:[]  
                        }
                    },
                    Datas: {
                        ...state.Datas,
                        UserList:action.datas
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
        default:
            return state
    }
}