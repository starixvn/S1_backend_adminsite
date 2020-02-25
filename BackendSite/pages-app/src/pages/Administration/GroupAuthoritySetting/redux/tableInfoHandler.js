import {reducerActions} from './actionsAndDataProperties'

const actionTypes = reducerActions.TableInfoActionTypes

export function TableInfoHandler(state, action){
    switch(action.type){
        case actionTypes.SET_TABLE_INFO:
            return Object.assign({}, state, {
                ...state,
                Datas: {
                    ...state.Datas,
                    TableDatas:{
                        ...state.Datas.TableDatas,
                        ...action.tableInfo
                    }
                }
            })
        default:
            return state
    }
}
