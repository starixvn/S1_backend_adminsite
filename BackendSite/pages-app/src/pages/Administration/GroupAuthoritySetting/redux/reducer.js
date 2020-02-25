import {combineReducers} from 'redux'
import {TableInfoHandler} from './tableInfoHandler'
import {PopboxHandler} from './popboxHandler'
import {InitDataModel} from './actionsAndDataProperties'
import {GroupAuthorityHandler} from './groupAuthorityHandler'
import {PermissionListHandler} from './permissionListHandler'

const initPopboxInfo = {
    PopboxName:null,
    Title:null,
    Content:null,
    ExecutingStatus:null,
    IsOpen:false,
    IdentityKey:null
}

const initCommonModel = {
    Datas:{
        InitGroupList:null,
        GroupList:null,
        Permissions:{
            PermissionDatas:null,
            HandleList:{
                CloseList_MainItemIdx:[],
                OpenList_ItemFuncKeys:[]
            }        
        },
        SortingInfo:null
    },
    Edit:InitDataModel.Edit,
    PopboxInfo:initPopboxInfo
}

export const DefineHandlers = {
    'TableInfoHandler':'TableInfoHandler',
    'PopboxHandler':'PopboxHandler',
    'GroupAuthorityHandler':'GroupAuthorityHandler',
    'PermissionListHandler':'PermissionListHandler'
}

function CombineHandler(state=initCommonModel, action){
    switch(action.handler){
        case DefineHandlers.TableInfoHandler:
            return TableInfoHandler(state, action)
        case DefineHandlers.PopboxHandler:
            return PopboxHandler(state, action)
        case DefineHandlers.GroupAuthorityHandler:
            return GroupAuthorityHandler(state, action)
        case DefineHandlers.PermissionListHandler:
            return PermissionListHandler(state, action)
        default:
            return initCommonModel;
    }
}

const Reducer = combineReducers({
    CommonHandler: CombineHandler
})

export default Reducer