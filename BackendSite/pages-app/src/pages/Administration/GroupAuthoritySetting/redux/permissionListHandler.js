import {reducerActions} from './actionsAndDataProperties'

const ActionTypes = reducerActions.PermissionListActionTypes

export function PermissionListHandler(state, action){
    switch(action.type){
        case ActionTypes.ENABLE_OR_DISABLE_FUNCTIONITEM:
            return SetPermissionData(state, action)
        case ActionTypes.ENABLE_OR_DISABLE_SUBOPTION:
            return SetPermissionData(state, action)
        case ActionTypes.UPDATE_PERMISSION_MODEL:
            return SetPermissionData(state, action)
        case ActionTypes.OPEN_OR_CLOSE_MAIN_LIST:
            return Object.assign({}, state, {
                ...state,
                Datas: {
                    ...state.Datas,
                    Permissions:{
                        ...state.Datas.Permissions,
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
            Permissions:{
                ...state.Datas.Permissions,
                PermissionDatas:action.datas
            }
        }
    })
}