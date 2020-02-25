import {reducerActions, InitDataModel} from './actionsAndDataProperties'

const ActionTypes = reducerActions.AssignCurrencyActionTypes
const initDataModel = InitDataModel.AssignCurrencyDataModel

export function AssignCurrencyHandler(state, action){
    switch(action.type){
        case ActionTypes.OPEN_POPUP_ASSIGN_CURRENCY:
                return Object.assign({}, state, {
                    ...state,
                    AssignCurrencyPopbox: {
                        ...state.AssignCurrencyPopbox,
                        SelectedCurr:action.selectedValue,
                        CurrencyList:action.currencyList
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
        case ActionTypes.CLOSE_POPUP_ASSIGN_CURRENCY:
            return ClosePopup(state, action)
        case ActionTypes.ON_SELECT_CURRENCY:
                return Object.assign({}, state, {
                    ...state,
                    AssignCurrencyPopbox: {
                        ...state.AssignCurrencyPopbox,
                        SelectedCurr:action.selectedValue
                    }
                })
        case ActionTypes.ASSIGN_CURRENCY:
            return ClosePopup(state, action);
        default:
            return state
    }
}

function ClosePopup(state, action){
    return Object.assign({}, state, {
        ...state,
        AssignCurrencyPopbox: {
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