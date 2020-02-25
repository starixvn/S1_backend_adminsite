export const webAPIUrl = {
    GET_USER_LIST:'Admin/GetUserList',
    CLICK_ENABLE_USER:'Admin/EnabledOrDisableUser',
    GET_GROUP_LIST:'Admin/GetAssignGroupList',
    ASSIGN_GROUP:'Admin/AssignGroup',
    GET_CURRENCY_LIST:'Admin/GetAssignCurrencyList',
    ASSIGN_CURRENCY:'Admin/AssignCurrnecy',
    RESET_PASSWORD:'Admin/ChangePassword',
    ADD_USER:'Admin/AddUser',
    GET_USER_PERMISSION:'Admin/GetUserPermission',
    AssignIndividualPermission:'Admin/AssignUserPermission',
    GET_GROUP_LIST_FROM_ADDUSER:'Admin/GetGroupListFROMAddUser',
    GET_USER_LIST_BY_CURRENCY:'Admin/GetUserListByCurrency',
    GET_LOGIN_USER_CURRENCY:'Admin/GetLoginUserCurrency',
    ADD_CURRENCY:'Admin/AddCurrency',
    REMOVE_CURRENCY:'Admin/RemoveCurrency',
    DELETE_USER:'Admin/DeleteUser'
}


export const reducerPropDefines = {
    PopboxNames:{
        'AddUser':'AddUser',
        'AssignGroup':'AssignGroup',
        'AssignIndividualPermission':'AssignIndividualPermission',
        'AssignCurrency':'AssignCurrency',
        'ResetPassword':'ResetPassword'
    },
    AddUserDataModelProperty: {
        'UserID':'UserID',
        'UserName':'UserName',
        'UserPWD':'UserPWD',
        'Email':'Email',
        'Currency':'Currency',
        'GroupName':'GroupName'
    }
}

export const InitDataModel = {
    UserPermissions:{
        PermissionDatas:null,
        HandleList:{
            CloseList_MainItemIdx:[],
            OpenList_ItemFuncKeys:[]
        }        
    },
    AddUserDataModel:{
        CurrencyList:[],
        AddUserDataModel:{
            [reducerPropDefines.AddUserDataModelProperty.UserID]: "",
            [reducerPropDefines.AddUserDataModelProperty.UserName]: "", 
            [reducerPropDefines.AddUserDataModelProperty.UserPWD]: "",
            [reducerPropDefines.AddUserDataModelProperty.Email]: "", 
            [reducerPropDefines.AddUserDataModelProperty.Currency]:[]  
        }      
    },
    AssignGroupDataModel:{
        GroupList:null,
        CurrentGroupSEQ:null,
        CurrentGroupName:null
    },
    AssignCurrencyDataModel:{
        SelectedCurr:[],
        CurrencyList:null
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
        'UPDATE_CURRENCY_LIST':'UPDATE_CURRENCY_LIST',
        'UPDATE_GROUP_LIST':'UPDATE_GROUP_LIST',
        'UPDATE_WHITE_LABEL_LIST':'UPDATE_WHITE_LABEL_LIST'
    },
    AssignGroupActionTypes:{
        'ASSIGN_GROUP':'ASSIGN_GROUP',
        'ON_SELECT_GROUP':'ON_SELECT_GROUP',
        'CLOSE_POPUP_ASSIGN_GROUP':'CLOSE_POPUP_ASSIGN_GROUP',
        'OPEN_POPUP_ASSIGN_GROUP':'OPEN_POPUP_ASSIGN_GROUP'
    },
    AssignIndividualPermissionActionTypes:{
        'OPEN_POPUP_ASSIGN_USER_PERMISSION':'OPEN_POPUP_ASSIGN_USER_PERMISSION',
        'OPEN_OR_CLOSE_MAIN_LIST':'OPEN_OR_CLOSE_MAIN_LIST',
        'ENABLE_OR_DISABLE_FUNCTIONITEM':'ENABLE_OR_DISABLE_FUNCTIONITEM',
        'ENABLE_OR_DISABLE_SUBOPTION':'ENABLE_OR_DISABLE_SUBOPTION',
        'ASSIGN_USER_PERMISSION':'ASSIGN_USER_PERMISSION',
        'RESET_TO_GROUP_SETTING':'RESET_TO_GROUP_SETTING',
        'CLOSE_POPUP_ASSIGN_USER_PERMISSION':'CLOSE_POPUP_ASSIGN_USER_PERMISSION'
    },
    AssignCurrencyActionTypes:{
        'OPEN_POPUP_ASSIGN_CURRENCY':'OPEN_POPUP_ASSIGN_CURRENCY',
        'ON_SELECT_CURRENCY':'ON_SELECT_CURRENCY',
        'ASSIGN_CURRENCY':'ASSIGN_CURRENCY',
        'CLOSE_POPUP_ASSIGN_CURRENCY':'CLOSE_POPUP_ASSIGN_CURRENCY',
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