import {reducerActions, InitDataModel} from './actionsAndDataProperties'

const ActionTypes = reducerActions.ResetPasswordActionTypes
const initDataModel = InitDataModel.ResetPasswordDataModel

export function ResetPasswordHandler(state, action){
    switch(action.type){
        case ActionTypes.OPEN_POPUP_RESET_PASSWORD:
                return Object.assign({}, state, {
                    ...state,
                    ResetPasswordPopbox: {
                        ...state.ResetPasswordPopbox,
                        ...initDataModel
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
        case ActionTypes.CLOSE_POPUP_RESET_PASSWORD:
            return ClosePopup(state, action)
        case ActionTypes.EDIT_RESET_PWD_FIELD:
                return Object.assign({}, state, {
                    ...state,
                    ResetPasswordPopbox: {
                        ...state.ResetPasswordPopbox,
                        NewPassword:action.value
                    }
                })
        case ActionTypes.RESET_PASSWORD:
            return Object.assign({}, state, {
                    ...state,
                    PopboxInfo: {
                        ...state.PopboxInfo,
                        ExecutingStatus:'success'
                    }
                })
        default:
            return state
    }
}

function ClosePopup(state, action){
    return Object.assign({}, state, {
        ...state,
        ResetPasswordPopbox: {
            ...initDataModel
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