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
        case ActionTypes.UPDATE_SITE_LIST:
                return Object.assign({}, state, {
                    ...state,
                    AddUserPopbox: {
                        ...state.AddUserPopbox,
                        SiteList:action.siteList
                    }
                })
        case ActionTypes.CLOSE_ADD_USER_POPBOX:
                return Object.assign({}, state, {
                    ...state,
                    AddUserPopbox:{
                        SiteList:[],
                        GroupList:[],
                        AddUserDataModel:{
                            [reducerPropDefines.AddUserDataModelProperty.userName]: "", 
                            [reducerPropDefines.AddUserDataModelProperty.userPWD]: "",
                            [reducerPropDefines.AddUserDataModelProperty.sites]:[]  
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
                        SiteList:[],
                        GroupList:[],
                        AddUserDataModel:{
                            [reducerPropDefines.AddUserDataModelProperty.userName]: "", 
                            [reducerPropDefines.AddUserDataModelProperty.userPWD]: "",
                            [reducerPropDefines.AddUserDataModelProperty.sites]:[]  
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