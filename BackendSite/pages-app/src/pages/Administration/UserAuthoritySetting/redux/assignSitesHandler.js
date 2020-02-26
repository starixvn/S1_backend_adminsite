import {reducerActions, InitDataModel} from './actionsAndDataProperties'

const ActionTypes = reducerActions.AssignSitesActionTypes
const initDataModel = InitDataModel.AssignSitesDataModel

export function AssignSitesHandler(state, action){
    switch(action.type){
        case ActionTypes.OPEN_POPUP_ASSIGN_SITES:
                return Object.assign({}, state, {
                    ...state,
                    AssignSitesPopbox: {
                        ...state.AssignSitesPopbox,
                        SiteList:action.siteList
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
        case ActionTypes.CLOSE_POPUP_ASSIGN_SITES:
            return ClosePopup(state, action)
        case ActionTypes.ON_SELECT_SITES:
                return Object.assign({}, state, {
                    ...state,
                    AssignSitesPopbox: {
                        ...state.AssignSitesPopbox,
                        SiteList:action.siteList
                    }
                })
        case ActionTypes.ASSIGN_SITES:
            return ClosePopup(state, action);
        default:
            return state
    }
}

function ClosePopup(state, action){
    return Object.assign({}, state, {
        ...state,
        AssignSitesPopbox: {
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