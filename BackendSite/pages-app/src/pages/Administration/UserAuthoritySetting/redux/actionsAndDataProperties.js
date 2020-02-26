export const webAPIUrl = {
    GET_USER_LIST:'AdminUser/GetUserList',
    GET_SITE_LIST:'AdminUser/GetSiteList',
    ASSIGN_SITES:'AdminUser/AssignSiteList',
    GET_GROUP_LIST:'AdminUser/GetGroupList',
    ASSIGN_GROUP:'AdminUser/AssignGroup',
    RESET_PASSWORD:'AdminUser/UpdatePassword',
    CLICK_ENABLE_USER:'AdminUser/UpdateStatus',
    GET_ALL_SITE_LIST:'AdminUser/GetAllSiteList',
    ADD_USER:'AdminUser/AddUser'
}

export const reducerPropDefines = {
    PopboxNames:{
        'AddUser':'AddUser',
        'AssignGroup':'AssignGroup',
        'AssignSites':'AssignSites',
        'ResetPassword':'ResetPassword'
    },
    AddUserDataModelProperty: {
        'userName':'userName',
        'userPWD':'userPWD',
        'sites':'sites'
    }
}

export const InitDataModel = {    
    AddUserDataModel:{
        SiteList:[],
        AddUserDataModel:{
            [reducerPropDefines.AddUserDataModelProperty.userName]: "", 
            [reducerPropDefines.AddUserDataModelProperty.userPWD]: "",
            [reducerPropDefines.AddUserDataModelProperty.sites]:[]  
        }      
    },
    AssignGroupDataModel:{
        GroupList:null,
        CurrentGroupSEQ:null,
        CurrentGroupName:null
    },
    AssignSitesDataModel:{
        SelectedSites:[],
        SiteList:null
    },
    ResetPasswordDataModel:{
        NewPassword:''
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
    AddUserActionTypes:{
        'OPEN_ADD_USER_POPBOX':'OPEN_ADD_USER_POPBOX',
        'CLOSE_ADD_USER_POPBOX':'CLOSE_ADD_USER_POPBOX',
        'ADD_USER':'ADD_USER',
        'EDIT_FIELD':'EDIT_FIELD',
        'UPDATE_SITE_LIST':'UPDATE_SITE_LIST',
    },
    AssignGroupActionTypes:{
        'ASSIGN_GROUP':'ASSIGN_GROUP',
        'ON_SELECT_GROUP':'ON_SELECT_GROUP',
        'CLOSE_POPUP_ASSIGN_GROUP':'CLOSE_POPUP_ASSIGN_GROUP',
        'OPEN_POPUP_ASSIGN_GROUP':'OPEN_POPUP_ASSIGN_GROUP'
    },
    AssignSitesActionTypes:{
        'OPEN_POPUP_ASSIGN_SITES':'OPEN_POPUP_ASSIGN_SITES',
        'ON_SELECT_SITES':'ON_SELECT_SITES',
        'ASSIGN_SITES':'ASSIGN_SITES',
        'CLOSE_POPUP_ASSIGN_SITES':'CLOSE_POPUP_ASSIGN_SITES',
    },
    ResetPasswordActionTypes:{
        'OPEN_POPUP_RESET_PASSWORD':'OPEN_POPUP_RESET_PASSWORD',
        'CLOSE_POPUP_RESET_PASSWORD':'CLOSE_POPUP_RESET_PASSWORD',
        'RESET_PASSWORD':'RESET_PASSWORD',
        'EDIT_RESET_PWD_FIELD':'EDIT_RESET_PWD_FIELD'
    },
    UserAuthoritySettingActionTypes:{
        'CLICK_ENABLE_USER':'CLICK_ENABLE_USER',
        'GET_USER_LIST':'GET_USER_LIST',
        'CLICK_DELETE_USER':'CLICK_DELETE_USER'
    }
}