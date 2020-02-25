export const webAPIUrl = {
    GetGroupList:'AdminGroup/GetGroupList',
    GetPermissionDatas:'Admin/GetPermissionDatas',
    DeleteGroup:'AdminGroup/DeleteGroup',
    SaveGroup:'AdminGroup/AddGroup',
    GetGroupInfo:'AdminGroup/GetEditGroup',
    UpdateGroup:'AdminGroup/UpdateGroup'
}

export const InitDataModel = {
    Permissions:{
        PermissionDatas:null,
        HandleList:{
            CloseList_MainItemIdx:[],
            OpenList_ItemFuncKeys:[]
        }        
    },
    Edit:{
        ShowPanel:false,
        IdentityKey:null,
        GroupInfo:{
            groupSEQ:null,
            groupName:null,
            remark:null,
        }
    }
}

export const reducerPropDefines = {
    GroupInfoProperty: {
        'groupName':'groupName',
        'remark':'remark',
        'groupSEQ':'groupSEQ'
    }
}

export const reducerActions = {
    TableInfoActionTypes:{
        'SET_TABLE_INFO':'SET_TABLE_INFO'
    },
    PopboxActionTypes:{
        'OPEN_POPBOX':'OPEN_POPBOX',
        'CLOSE_POPBOX':'CLOSE_POPBOX',
        'EDIT_POPBOX_CONTENT':'EDIT_POPBOX_CONTENT',
        'UPDATE_EXECUTING_STATUS':'UPDATE_EXECUTING_STATUS',
        'CLOSE_TIPS':'CLOSE_TIPS'
    },
    GroupAuthorityActionTypes:{
        'GET_GROUP_LIST':'GET_GROUP_LIST',
        'CLICK_DELETE_GROUP':'CLICK_DELETE_GROUP',
        'CLICK_EDIT_GROUP':'CLICK_EDIT_GROUP',
        'CLICK_ADD_GROUP':'CLICK_ADD_GROUP',
        'CLOSE_EDIT_PANEL':'CLOSE_EDIT_PANEL',
        'EDIT_GROUP_INFO_FIELD':'EDIT_GROUP_INFO_FIELD',
        'ADD_GROUP':'ADD_GROUP',
        'UPDATE_GROUP':'UPDATE_GROUP',
        'EDIT_GROUP':'EDIT_GROUP',
        'CLICK_SORTING':'CLICK_SORTING'
    },
    PermissionListActionTypes:{
        'OPEN_OR_CLOSE_MAIN_LIST':'OPEN_OR_CLOSE_MAIN_LIST',
        'ENABLE_OR_DISABLE_FUNCTIONITEM':'ENABLE_OR_DISABLE_FUNCTIONITEM',
        'ENABLE_OR_DISABLE_SUBOPTION':'ENABLE_OR_DISABLE_SUBOPTION',
        'UPDATE_PERMISSION_MODEL':'UPDATE_PERMISSION_MODEL'
    }
}