import {reducerActions, InitDataModel, reducerPropDefines} from './actionsAndDataProperties'

const ActionTypes = reducerActions.AssignGroupActionTypes
const initDataModel = InitDataModel.AssignGroupDataModel

export function AssignGroupHandler(state, action){
    switch(action.type){
        case ActionTypes.OPEN_POPUP_ASSIGN_GROUP:
                return Object.assign({}, state, {
                    ...state,
                    AssignGroupPopbox: {
                        ...state.AssignGroupPopbox,
                        CurrentGroupSEQ:action.currentGroupSEQ,
                        GroupList:action.groupList,
                        CurrentGroupName:action.currentGroupName
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
        case ActionTypes.CLOSE_POPUP_ASSIGN_GROUP:
                return ClosePopup(state)
        case ActionTypes.ON_SELECT_GROUP:
                return Object.assign({}, state, {
                    ...state,
                    AssignGroupPopbox: {
                        ...state.AssignGroupPopbox,
                        CurrentGroupSEQ:action.currentGroupSEQ,
                        CurrentGroupName:action.currentGroupName
                    }
                })
        case ActionTypes.ASSIGN_GROUP:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        UserList:action.UserList
                    },
                    AssignGroupPopbox: {
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
        default:
            return state
    }
}

function ClosePopup(state){
    return Object.assign({}, state, {
        ...state,
        AssignGroupPopbox: {
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