import {reducerActions} from './actionsAndDataProperties'

const ActionTypes = reducerActions.AssignIndividualPermissionActionTypes

export function AssignIndividualPermissionHandler(state, action){
    switch(action.type){
        case ActionTypes.OPEN_POPUP_ASSIGN_USER_PERMISSION:
            return Object.assign({}, state, {
                ...state,
                Datas: {
                    ...state.Datas,
                    UserPermissions:{
                        ...state.Datas.UserPermissions,
                        PermissionDatas:action.datas
                    }
                },
                PopboxInfo: {
                    ...state.PopboxInfo,
                    PopboxName:action.popboxName,
                    Title:action.title,
                    ExecutingStatus:null,
                    IsOpen:true,
                    IdentityKey:action.identityKey
                }
            })
        case ActionTypes.ASSIGN_USER_PERMISSION:
            return CloseUserPermissionPopup(state, action)
        case ActionTypes.RESET_TO_GROUP_SETTING:
            return SetPermissionData(state, action)
        case ActionTypes.ENABLE_OR_DISABLE_FUNCTIONITEM:
            return SetPermissionData(state, action)
        case ActionTypes.ENABLE_OR_DISABLE_SUBOPTION:
            return SetPermissionData(state, action)
        case ActionTypes.CLOSE_POPUP_ASSIGN_USER_PERMISSION:
            return CloseUserPermissionPopup(state, action)
        case ActionTypes.OPEN_OR_CLOSE_MAIN_LIST:
            return Object.assign({}, state, {
                ...state,
                Datas: {
                    ...state.Datas,
                    UserPermissions:{
                        ...state.Datas.UserPermissions,
                        HandleList:action.datas
                    }
                }
            })
        
        default:
            return state
    }
}

function SetPermissionData(state, action){
    return Object.assign({}, state, {
        ...state,
        Datas: {
            ...state.Datas,
            UserPermissions:{
                ...state.Datas.UserPermissions,
                PermissionDatas:action.datas
            }
        }
    })
}

function CloseUserPermissionPopup(state, action){
    return Object.assign({}, state, {
        ...state,
        Datas: {
            ...state.Datas,
            UserPermissions:action.datas
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
}