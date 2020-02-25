import {reducerActions} from './actionsAndDataProperties'

const ActionTypes = reducerActions.GroupAuthorityActionTypes

export function GroupAuthorityHandler(state, action){
    switch(action.type){
        case ActionTypes.GET_GROUP_LIST:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        GroupList:action.datas,
                        InitGroupList:action.datas
                    }
                })
        case ActionTypes.CLICK_DELETE_GROUP:
            return Object.assign({}, state, {
                ...state,
                Datas: {
                    ...state.Datas,
                    GroupList:action.datas,
                    InitGroupList:action.initDatas
                }
            })
        case ActionTypes.CLICK_ADD_GROUP:
            return Object.assign({}, state, {
                ...state,
                Datas: {
                    ...state.Datas,
                    Permissions:{
                        ...state.Datas.Permissions,
                        PermissionDatas:action.permissionDatas
                    }
                },
                Edit: {
                    ...state.Edit,
                    ShowPanel:true,
                    ActionType:action.actionType,
                }
            })
        case ActionTypes.ADD_GROUP:
            return Object.assign({}, state, {
                ...state,
                Datas: {
                    ...state.Datas,
                    GroupList:action.datas,
                    InitGroupList:action.datas,
                    Permissions:{
                        PermissionDatas:null,
                        HandleList:{
                            CloseList_MainItemIdx:[],
                            OpenList_ItemFuncKeys:[]
                        }        
                    }
                },
                Edit: {
                    ...state.Edit,
                    ShowPanel:false,
                    ActionType:null,
                    GroupInfo:{
                        groupSEQ:null,
                        groupName:null,
                        remark:null
                    }
                }
            })
        case ActionTypes.CLICK_EDIT_GROUP:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        Permissions:{
                            ...state.Datas.Permissions,
                            PermissionDatas:action.permissionDatas
                        }
                    },
                    Edit: {
                        ...state.Edit,
                        ShowPanel:true,
                        ActionType:action.actionType,
                        IdentityKey:action.identityKey,
                        GroupInfo:action.groupInfo
                    }
                })
        case ActionTypes.EDIT_GROUP:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        GroupList:action.datas,
                        InitGroupList:action.datas,
                        Permissions:{
                            PermissionDatas:null,
                            HandleList:{
                                CloseList_MainItemIdx:[],
                                OpenList_ItemFuncKeys:[]
                            }        
                        }
                    },
                    Edit: {
                        ...state.Edit,
                        ShowPanel:false,
                        ActionType:null,
                        GroupInfo:{
                            groupSEQ:null,
                            groupName:null,
                            remark:null
                        }
                    }
                })
        case ActionTypes.EDIT_GROUP_INFO_FIELD:
                return Object.assign({}, state, {
                    ...state,
                    Edit: {
                        ...state.Edit,
                        GroupInfo:action.groupInfo
                    }
                })
        case ActionTypes.CLOSE_EDIT_PANEL:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        Permissions:{
                            PermissionDatas:null,
                            HandleList:{
                                CloseList_MainItemIdx:[],
                                OpenList_ItemFuncKeys:[]
                            }        
                        }
                    },
                    Edit: {
                        ...state.Edit,
                        ShowPanel:false,
                        ActionType:null,
                        GroupInfo:{
                            groupSEQ:null,
                            groupName:null,
                            remark:null
                        }
                    }
                })
        case ActionTypes.CLICK_SORTING:
                return Object.assign({}, state, {
                    ...state,
                    Datas: {
                        ...state.Datas,
                        GroupList:action.GroupList,
                        InitGroupList:action.InitGroupList,
                        SortingInfo:{
                            [action.column]:action.sortingStatus
                        }
                    }
                })
        default:
            return state
    }
}