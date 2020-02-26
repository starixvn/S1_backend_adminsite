import {reducerActions} from './actionsAndDataProperties'

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
        default:
            return state
    }
}