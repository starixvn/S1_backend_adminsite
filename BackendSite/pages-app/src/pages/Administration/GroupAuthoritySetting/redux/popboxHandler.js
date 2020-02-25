import {reducerActions} from './actionsAndDataProperties'

const ActionTypes = reducerActions.PopboxActionTypes

export function PopboxHandler(state, action){
    switch(action.type){
        case ActionTypes.OPEN_POPBOX:
                return Object.assign({}, state, {
                    ...state,
                    PopboxInfo: {
                        ...state.PopboxInfo,
                        PopboxName:action.popboxName,
                        Title:action.title,
                        Content:action.content,
                        ExecutingStatus:null,
                        IdentityKey:action.identityKey,
                        IsOpen:true
                    }
                })
        case ActionTypes.CLOSE_POPBOX:
                return Object.assign({}, state, {
                    ...state,
                    PopboxInfo: {
                        PopboxName:null,
                        Title:null,
                        Content:null,
                        ExecutingStatus:null,
                        IsOpen:false,
                        IdentityKey:null
                    }
                })
        case ActionTypes.EDIT_POPBOX_CONTENT:
                return Object.assign({}, state, {
                    ...state,
                    PopboxInfo: {
                        ...state.PopboxInfo,
                        Content:action.content
                    }
                })
        case ActionTypes.CLOSE_TIPS:
                return Object.assign({}, state, {
                    ...state,
                    PopboxInfo: {
                        ...state.PopboxInfo,
                        ExecutingStatus:null
                    }
                })
        case ActionTypes.UPDATE_EXECUTING_STATUS:
                return Object.assign({}, state, {
                    ...state,
                    PopboxInfo: {
                        ...state.PopboxInfo,
                        ExecutingStatus:action.executingStatus
                    }
                })
        default:
            return state
    }
}