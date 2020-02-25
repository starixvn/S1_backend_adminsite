import {createStore} from 'redux'
import Reducer, {DefineHandlers} from './reducer'
import {reducerPropDefines, reducerActions, webAPIUrl, InitDataModel} from './actionsAndDataProperties'
import { composeWithDevTools } from 'redux-devtools-extension'
import WebAPI from '../../../../module/WebAPI'
import Lang from '../../../../resource/Administration/AuthoritySetting/LangAPI'
import {GetAddUserDefineElement} from '../PopboxContentAddUser'
import {VaildFieldValues} from '../../../../module/ValidFieldValue'

export let store = process.env.NODE_ENV==='production' ? createStore(Reducer) : createStore(Reducer, composeWithDevTools());

export function mapStateToProps(state){
    let props = {};
    Object.keys(state.CommonHandler).forEach(key=>{
        props[key] = state.CommonHandler[key]
    })

    return props;
}

function AddUserProps(dispatch){
    return {
        [reducerActions.AddUserActionTypes.OPEN_ADD_USER_POPBOX]:()=>{
            dispatch({
                handler:DefineHandlers.AddUserHandler,
                type:reducerActions.AddUserActionTypes.OPEN_ADD_USER_POPBOX,
                title:Lang.getLang(window.LangCode,"lbl_add_user"),
                popboxName:reducerPropDefines.PopboxNames.AddUser
            })
        },
        [reducerActions.AddUserActionTypes.CLOSE_ADD_USER_POPBOX]:()=>{
            dispatch({
                handler:DefineHandlers.AddUserHandler,
                type:reducerActions.AddUserActionTypes.CLOSE_ADD_USER_POPBOX
            })
        },
        [reducerActions.AddUserActionTypes.EDIT_FIELD]:(value, datacolumn)=>{
            let datas = store.getState().CommonHandler.AddUserPopbox.AddUserDataModel;
            let sameValue = datas[datacolumn] === value;

            if(!sameValue){
                switch(datacolumn){
                    case reducerPropDefines.AddUserDataModelProperty.Currency:
                        let v = Number(value) 
                        let find_idx = datas[datacolumn].indexOf(v)
                        if(find_idx === -1){
                            datas[datacolumn].push(v)
                        }else{
                            datas[datacolumn].splice(find_idx, 1)
                        }
                        break;

                    default:
                        datas[datacolumn] = value;
                }

            
                dispatch({
                    handler:DefineHandlers.AddUserHandler,
                    type:reducerActions.AddUserActionTypes.EDIT_FIELD,
                    datas:datas
                })
            }
        },
        [reducerActions.AddUserActionTypes.ADD_USER]:()=>{
            let newUser = store.getState().CommonHandler.AddUserPopbox.AddUserDataModel;

            let defineElems = GetAddUserDefineElement();
            let checked = VaildFieldValues(newUser, defineElems)
            if(checked){
                let param = {
                    Url: webAPIUrl.ADD_USER,
                    Data: {
                        User:newUser,
                        Currencies:newUser.Currency,
                        GroupSEQ:newUser.GroupName.SEQ
                    }
                };
    
                WebAPI.PostData(param).then((res) => {
                    if (res.data.ErrCode === 0) {
                        dispatch({
                            handler:DefineHandlers.AddUserHandler,
                            type:reducerActions.AddUserActionTypes.ADD_USER,
                            datas:res.data.Data
                        })
                    } else {
                        alert(res.data.ErrMsg)
                    }
                });
            }            
        }
    }
}

function UserAuthoritySettingProps(dispatch){
    return {
        [reducerActions.UserAuthoritySettingActionTypes.GET_USER_LIST]:()=>{
            let param = {
                Url: webAPIUrl.GET_USER_LIST
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.ErrCode === 0) {
                    dispatch({
                        handler:DefineHandlers.UserAuthoritySettingHandler,
                        type:reducerActions.UserAuthoritySettingActionTypes.GET_USER_LIST,
                        datas:res.data.Data
                    })
                } else {
                    alert(res.data.ErrMsg)
                }
            })
        },
        [reducerActions.UserAuthoritySettingActionTypes.CLICK_ENABLE_USER]:(v, idx)=>{
            let datas = store.getState().CommonHandler.Datas.UserList;
            let isEnable = v==='false';

            let param = {
                Url: webAPIUrl.CLICK_ENABLE_USER,
                Data: {
                    UserSEQ:datas[idx].UserSEQ,
                    isEnabled:isEnable
                }
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.ErrCode === 0) {
                    dispatch({
                        handler:DefineHandlers.UserAuthoritySettingHandler,
                        type:reducerActions.UserAuthoritySettingActionTypes.CLICK_ENABLE_USER,
                        UserList:datas
                    })
                } else {
                    alert(res.data.ErrMsg)
                }
            })
        },
        [reducerActions.UserAuthoritySettingActionTypes.CLICK_DELETE_USER]:(tableName, idx)=>{
            let datas = store.getState().CommonHandler.Datas;
            
            if(window.confirm(Lang.getLang(window.LangCode,"msg_delete_user", [datas.UserList[idx].UserID]))){
                let userSEQ = datas.UserList[idx].UserSEQ;    
                let param = {
                    Url: webAPIUrl.DELETE_USER,
                    Data: {
                        UserSEQ:userSEQ,
                    }
                };

                WebAPI.PostData(param).then((res) => {
                    if (res.data.ErrCode === 0) {
                        let UserList = datas.UserList;
                        dispatch({
                            handler:DefineHandlers.UserAuthoritySettingHandler,
                            type:reducerActions.UserAuthoritySettingActionTypes.CLICK_DELETE_USER,
                            UserList:UserList
                        });
                    } else {
                        alert(res.data.ErrMsg)
                    }
                });
            }
        }
    }
}

function EnableOrDisableAllFunctionKeys(datas, checked){
    for(let i in datas){
        let setting = datas[i]
        if(setting.IsGroupSetting && !setting.FunctionKeys){
            continue;
        }

        setting.Enable = setting.IsGroupSetting ? setting.Enable : checked
        setting.UserCheckedIndex = !checked ? [] : setting.UserCheckedIndex
        if(setting.FunctionKeys){
            setting.FunctionKeys = EnableOrDisableAllFunctionKeys(setting.FunctionKeys, checked);
        }
    }

    return datas
}

function EnableOrDisableFunctionKeys(datas, checked, FuncKey){
    for(let i in datas){
        let setting = datas[i]
        if(setting.IsGroupSetting && !setting.FunctionKeys){
            continue
        }
        
        if(setting.FunctionKey===FuncKey){
            setting.Enable = setting.IsGroupSetting ? setting.Enable : checked
            setting.FunctionKeys = !checked && setting.FunctionKeys ? EnableOrDisableAllFunctionKeys(setting.FunctionKeys, checked) : setting.FunctionKeys
            setting.UserCheckedIndex = !checked ? [] : setting.UserCheckedIndex
            break;
        }else if(setting.FunctionKeys){
            setting.FunctionKeys = EnableOrDisableFunctionKeys(setting.FunctionKeys, checked, FuncKey)
        }
    }

    return datas
}

function EnableOrDisableSubOption(datas, optIdx, FuncKey){
    for(let i in datas){
        let setting = datas[i]
        if(setting.FunctionKey===FuncKey){
            if(setting.InputBoxType==='radio' && setting.IsGroupSetting) continue;
            let find_idx = setting.UserCheckedIndex && setting.UserCheckedIndex.indexOf(optIdx)
            if(find_idx===-1){
                if(setting.InputBoxType==='radio'){
                    setting.UserCheckedIndex=[optIdx]
                }else{
                    setting.UserCheckedIndex.push(optIdx)
                }
            }else if(find_idx===null){
                setting.UserCheckedIndex = [optIdx]
            }else{
                if(setting.InputBoxType==='radio'){
                    setting.UserCheckedIndex=[]
                }else{
                    setting.UserCheckedIndex.splice(find_idx, 1)
                }
            }
            
            break;
        }else if(setting.FunctionKeys){
            setting.FunctionKeys = EnableOrDisableSubOption(setting.FunctionKeys, optIdx, FuncKey)
        }
    }

    return datas
}

function GetTitleDisplayText(PopNameLangKey, UserID){
    return `${Lang.getLang(window.LangCode, PopNameLangKey)} - ${Lang.getLang(window.LangCode,"lbl_UserID")}：${UserID}`;
}

function AssignIndividualPermissionProps(dispatch){
    return {
        [reducerActions.AssignIndividualPermissionActionTypes.OPEN_POPUP_ASSIGN_USER_PERMISSION]:(tableName, idx)=>{            
            let datas = store.getState().CommonHandler.Datas.UserList;
            let param = {
                Url: webAPIUrl.GET_USER_PERMISSION,
                Data: {
                    UserSEQ:datas[idx].UserSEQ
                }
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.ErrCode === 0) {
                    let title = GetTitleDisplayText("lbl_individual", datas[idx].UserID);
                    dispatch({
                        handler:DefineHandlers.AssignIndividualPermissionHandler,
                        type:reducerActions.AssignIndividualPermissionActionTypes.OPEN_POPUP_ASSIGN_USER_PERMISSION,
                        title:title,
                        popboxName:reducerPropDefines.PopboxNames.AssignIndividualPermission,
                        datas:res.data.Data,
                        identityKey:{idx:idx}
                    })
                } else {
                    alert(res.data.ErrMsg)
                }
            });
        },
        [reducerActions.AssignIndividualPermissionActionTypes.OPEN_OR_CLOSE_MAIN_LIST]:(MainIdx)=>{
            let handleList = store.getState().CommonHandler.Datas.UserPermissions.HandleList;
            let find_idx = handleList.CloseList_MainItemIdx.indexOf(MainIdx)
            if(find_idx===-1){
                handleList.CloseList_MainItemIdx.push(MainIdx)
            }else{
                handleList.CloseList_MainItemIdx.splice(find_idx,1)
            }

            dispatch({
                handler:DefineHandlers.AssignIndividualPermissionHandler,
                type:reducerActions.AssignIndividualPermissionActionTypes.OPEN_OR_CLOSE_MAIN_LIST,
                datas:handleList
            })
        },
        [reducerActions.AssignIndividualPermissionActionTypes.ENABLE_OR_DISABLE_FUNCTIONITEM]:(v, param={FuncKey:null, MainIdx:null, IsTopItem:false})=>{
            let datas = store.getState().CommonHandler.Datas.UserPermissions.PermissionDatas;
            if(param){
                let checked = v==='false';
                let topItemidx = param.MainIdx;
                if(param.IsTopItem){                    
                    datas[topItemidx].Enable = checked
                    for(let i in datas[topItemidx].Settings){
                        let setting = datas[topItemidx].Settings[i]
                        if(setting.IsGroupSetting && !setting.FunctionKeys){
                            continue;
                        }
                        setting.Enable = setting.IsGroupSetting ? setting.Enable : checked
                        setting.FunctionKeys = setting.FunctionKeys ? EnableOrDisableAllFunctionKeys(setting.FunctionKeys, checked) : setting.FunctionKeys;
                    }
                }else{
                    datas[topItemidx].Settings = EnableOrDisableFunctionKeys(datas[topItemidx].Settings, checked, param.FuncKey)
                }
            }
            
            dispatch({
                handler:DefineHandlers.AssignIndividualPermissionHandler,
                type:reducerActions.AssignIndividualPermissionActionTypes.ENABLE_OR_DISABLE_FUNCTIONITEM,
                datas:datas
            })
        },
        [reducerActions.AssignIndividualPermissionActionTypes.ENABLE_OR_DISABLE_SUBOPTION]:(v, param={FuncKey:null, MainIdx:null, IsTopItem:false})=>{
            let datas = store.getState().CommonHandler.Datas.UserPermissions.PermissionDatas;
            datas[param.MainIdx].Settings = EnableOrDisableSubOption(datas[param.MainIdx].Settings, Number(v), param.FuncKey)
            dispatch({
                handler:DefineHandlers.AssignIndividualPermissionHandler,
                type:reducerActions.AssignIndividualPermissionActionTypes.ENABLE_OR_DISABLE_SUBOPTION,
                datas:datas
            })
        },
        [reducerActions.AssignIndividualPermissionActionTypes.RESET_TO_GROUP_SETTING]:()=>{
            let datas = store.getState().CommonHandler.Datas.UserPermissions.PermissionDatas;
            let func_settingLoop = (Settings)=>{
                for(let s in Settings){
                    let item = Settings[s];
                    if(item.FunctionKeys){
                        item.Enable = item.IsGroupSetting ? item.Enable : false;
                        item.FunctionKeys = func_settingLoop(item.FunctionKeys)
                    }else{
                        if(item.InputBoxType==='checkbox' || !item.IsGroupSetting){
                            item.UserCheckedIndex = null;
                            item.Enable = !item.GroupCheckedIndex ? false : item.Enable                            
                        }
                    }
                }

                return Settings;
            }
            for(let i in datas){
                datas[i].Settings = func_settingLoop(datas[i].Settings)                
            }

            dispatch({
                handler:DefineHandlers.AssignIndividualPermissionHandler,
                type:reducerActions.AssignIndividualPermissionActionTypes.RESET_TO_GROUP_SETTING,
                datas:datas
            })
        },
        [reducerActions.AssignIndividualPermissionActionTypes.ASSIGN_USER_PERMISSION]:()=>{
            if(window.confirm(Lang.getLang(window.LangCode,"msg_save_changes"))){
                let datas = store.getState().CommonHandler.Datas.UserPermissions.PermissionDatas;
                let userList = store.getState().CommonHandler.Datas.UserList;
                let _identityKey = store.getState().CommonHandler.PopboxInfo.IdentityKey;
    
                let param = {
                    Url: webAPIUrl.AssignIndividualPermission,
                    Data: {
                        UserSEQ:userList[_identityKey.idx].UserSEQ,
                        Permissions:JSON.stringify(datas)
                    }
                };
    
                WebAPI.PostData(param).then((res) => {
                    if (res.data.ErrCode === 0) {
                        dispatch({
                            handler:DefineHandlers.AssignIndividualPermissionHandler,
                            type:reducerActions.AssignIndividualPermissionActionTypes.ASSIGN_USER_PERMISSION,
                            datas:InitDataModel.UserPermissions
                        })
                    } else {
                        alert(res.data.ErrMsg)
                    }
                });
            }
            
        },
        [reducerActions.AssignIndividualPermissionActionTypes.CLOSE_POPUP_ASSIGN_USER_PERMISSION]:()=>{
            dispatch({
                handler:DefineHandlers.AssignIndividualPermissionHandler,
                type:reducerActions.AssignIndividualPermissionActionTypes.CLOSE_POPUP_ASSIGN_USER_PERMISSION,
                datas:InitDataModel.UserPermissions
            })
        }
    }
}

function ResetPasswordProps(dispatch){
    return {
        [reducerActions.ResetPasswordActionTypes.OPEN_POPUP_RESET_PASSWORD]:(tableName, idx)=>{
            let userList = store.getState().CommonHandler.Datas.UserList;
            dispatch({
                handler:DefineHandlers.ResetPasswordHandler,
                type:reducerActions.ResetPasswordActionTypes.OPEN_POPUP_RESET_PASSWORD,
                title:`${Lang.getLang(window.LangCode,"lbl_change_password")} - ${Lang.getLang(window.LangCode,"lbl_UserID")}：${userList[idx].UserID}`,
                popboxName:reducerPropDefines.PopboxNames.ResetPassword,
                identityKey:{idx:idx}
            })
        },
        [reducerActions.ResetPasswordActionTypes.EDIT_RESET_PWD_FIELD]:(value)=>{
            dispatch({
                handler:DefineHandlers.ResetPasswordHandler,
                type:reducerActions.ResetPasswordActionTypes.EDIT_RESET_PWD_FIELD,
                value:value
            })
        },
        [reducerActions.ResetPasswordActionTypes.CLOSE_POPUP_RESET_PASSWORD]:()=>{
            dispatch({
                handler:DefineHandlers.ResetPasswordHandler,
                type:reducerActions.ResetPasswordActionTypes.CLOSE_POPUP_RESET_PASSWORD
            })
        },
        [reducerActions.ResetPasswordActionTypes.RESET_PASSWORD]:()=>{
            if(window.confirm(Lang.getLang(window.LangCode,"msg_save_changes"))){
                let newPassword = store.getState().CommonHandler.ResetPasswordPopbox.NewPassword;
                let userList = store.getState().CommonHandler.Datas.UserList;
                let _identityKey = store.getState().CommonHandler.PopboxInfo.IdentityKey;

                let param = {
                    Url: webAPIUrl.RESET_PASSWORD,
                    Data: {
                        Password:newPassword,
                        UserSEQ:userList[_identityKey.idx].UserSEQ
                    }
                };

                WebAPI.PostData(param).then((res) => {
                    if (res.data.ErrCode === 0) {
                        dispatch({
                            handler:DefineHandlers.ResetPasswordHandler,
                            type:reducerActions.ResetPasswordActionTypes.RESET_PASSWORD,
                        })
                    } else {
                        alert(res.data.ErrMsg)
                    }
                });
            }
        }
    }
}

function AssignCurrencyProps(dispatch){
    return {
        [reducerActions.AssignCurrencyActionTypes.OPEN_POPUP_ASSIGN_CURRENCY]:(tableName, idx)=>{
            let datas = store.getState().CommonHandler.Datas.UserList;
            // let currencyList = [
                    //     { CurrencyId:20, CurrencyName:'UUS' },
                    //     { CurrencyId:13, CurrencyName:'RMB' },
                    //     { CurrencyId:25, CurrencyName:'VND' },
                    // ]

            let param = {
                Url: webAPIUrl.GET_CURRENCY_LIST,
                Data: {
                    UserSEQ:datas[idx].UserSEQ
                }
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.ErrCode === 0) {
                    let selectedValue = res.data.Selected;
                    let currencyList = res.data.CurrencyList;
                    let title =GetTitleDisplayText("lbl_currency", datas[idx].UserID); 
                    dispatch({
                        handler:DefineHandlers.AssignCurrencyHandler,
                        type:reducerActions.AssignCurrencyActionTypes.OPEN_POPUP_ASSIGN_CURRENCY,
                        title:title,
                        popboxName:reducerPropDefines.PopboxNames.AssignCurrency,
                        currencyList:currencyList,
                        selectedValue:selectedValue,
                        identityKey:{idx:idx}
                    })
                } else {
                    alert(res.data.ErrMsg)
                }
            })
        },
        [reducerActions.AssignCurrencyActionTypes.CLOSE_POPUP_ASSIGN_CURRENCY]:()=>{
            dispatch({
                handler:DefineHandlers.AssignCurrencyHandler,
                type:reducerActions.AssignCurrencyActionTypes.CLOSE_POPUP_ASSIGN_CURRENCY
            })
        },
        [reducerActions.AssignCurrencyActionTypes.ON_SELECT_CURRENCY]:(v)=>{
            let selectedValue = store.getState().CommonHandler.AssignCurrencyPopbox.SelectedCurr;
            selectedValue = selectedValue === null ? [] : selectedValue
            let currentSelect = Number(v);
            let find_idx = selectedValue.indexOf(currentSelect);
            if(find_idx===-1){
                selectedValue.push(currentSelect)
            }else{
                selectedValue.splice(find_idx, 1)
            }

            dispatch({
                handler:DefineHandlers.AssignCurrencyHandler,
                type:reducerActions.AssignCurrencyActionTypes.ON_SELECT_CURRENCY,
                selectedValue:selectedValue
            })
        },
        [reducerActions.AssignCurrencyActionTypes.ASSIGN_CURRENCY]:()=>{
            if(window.confirm(Lang.getLang(window.LangCode,"msg_save_changes"))){
                let userList = store.getState().CommonHandler.Datas.UserList;
                let selectedValue = store.getState().CommonHandler.AssignCurrencyPopbox.SelectedCurr;
                let _identityKey = store.getState().CommonHandler.PopboxInfo.IdentityKey;
                let param = {
                    Url: webAPIUrl.ASSIGN_CURRENCY,
                    Data: {
                        CurrencyIds:selectedValue,
                        UserSEQ:userList[_identityKey.idx].UserSEQ
                    }
                };

                WebAPI.PostData(param).then((res) => {
                    if (res.data.ErrCode === 0) {
                        dispatch({
                            handler:DefineHandlers.AssignCurrencyHandler,
                            type:reducerActions.AssignCurrencyActionTypes.ASSIGN_CURRENCY,
                        })
                    } else {
                        alert(res.data.ErrMsg)
                    }
                })
            }
        }
    }
}

function AssignGroupProps(dispatch){
    return {
        [reducerActions.AssignGroupActionTypes.OPEN_POPUP_ASSIGN_GROUP]:(tableName, idx)=>{
            let datas = store.getState().CommonHandler.Datas.UserList;
            
            let param = {
                Url: webAPIUrl.GET_GROUP_LIST,
                Data: {
                    UserSEQ:datas[idx].UserSEQ
                }
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.ErrCode === 0) {
                    let currentGroupSEQ = datas[idx].GroupSEQ;
                    let currentGroupName = datas[idx].GroupName;
                    let title = GetTitleDisplayText("lbl_role", datas[idx].UserID);
                    let groupList = res.data.Data;
                    dispatch({
                        handler:DefineHandlers.AssignGroupHandler,
                        type:reducerActions.AssignGroupActionTypes.OPEN_POPUP_ASSIGN_GROUP,
                        title:title,
                        popboxName:reducerPropDefines.PopboxNames.AssignGroup,
                        groupList:groupList,
                        currentGroupSEQ:currentGroupSEQ,
                        currentGroupName:currentGroupName,
                        identityKey:{idx:idx}
                    })
                } else {
                    alert(res.data.ErrMsg)
                }
            })
        },
        [reducerActions.AssignGroupActionTypes.CLOSE_POPUP_ASSIGN_GROUP]:()=>{
            dispatch({
                handler:DefineHandlers.AssignGroupHandler,
                type:reducerActions.AssignGroupActionTypes.CLOSE_POPUP_ASSIGN_GROUP
            })
        },
        [reducerActions.AssignGroupActionTypes.ON_SELECT_GROUP]:(v, GroupName)=>{
            dispatch({
                handler:DefineHandlers.AssignGroupHandler,
                type:reducerActions.AssignGroupActionTypes.ON_SELECT_GROUP,
                currentGroupSEQ:Number(v),
                currentGroupName:GroupName
            })
        },
        [reducerActions.AssignGroupActionTypes.ASSIGN_GROUP]:()=>{
            if(window.confirm(Lang.getLang(window.LangCode,"msg_save_changes"))){
                let datas = store.getState().CommonHandler.Datas.UserList;
                let _identityKey = store.getState().CommonHandler.PopboxInfo.IdentityKey;
                let new_group = store.getState().CommonHandler.AssignGroupPopbox;
                
                let param = {
                    Url: webAPIUrl.ASSIGN_GROUP,
                    Data: {
                        GroupSEQ:new_group.CurrentGroupSEQ,
                        UserSEQ:datas[_identityKey.idx].UserSEQ
                    }
                };

                WebAPI.PostData(param).then((res) => {
                    if (res.data.ErrCode === 0) {
                        datas[_identityKey.idx].GroupName = new_group.CurrentGroupName;
                        datas[_identityKey.idx].GroupSEQ = new_group.CurrentGroupSEQ;

                        dispatch({
                            handler:DefineHandlers.AssignGroupHandler,
                            type:reducerActions.AssignGroupActionTypes.ASSIGN_GROUP,
                            UserList:datas
                        })
                    } else {
                        alert(res.data.ErrMsg)
                    }
                })
            }
        }
    }
}

function PopboxProps(dispatch){
    return {
        [reducerActions.PopboxActionTypes.CLOSE_POPBOX]:()=>{
            dispatch({
                handler:DefineHandlers.PopboxHandler,
                type:reducerActions.PopboxActionTypes.CLOSE_POPBOX
            })
        },
        [reducerActions.PopboxActionTypes.CLOSE_TIPS]:()=>{
            dispatch({
                handler:DefineHandlers.PopboxHandler,
                type:reducerActions.PopboxActionTypes.CLOSE_TIPS
            })
        }
    }
}

export function mapDispatchToProps(dispatch){

    let result = {
        [reducerActions.TableInfoActionTypes.SET_TABLE_INFO]:(tableName)=>{
            let resultDatas = null
            
            let tableInfo = {
                [tableName]: {
                    ResultDatas:resultDatas
                }
            }

            dispatch({
                handler:DefineHandlers.TableInfoHandler,
                type:reducerActions.TableInfoActionTypes.SET_TABLE_INFO,                
                tableInfo:tableInfo
            })
        },
    }

    return Object.assign({}, result, {
                ...result,
                ...AddUserProps(dispatch),
                ...AssignGroupProps(dispatch),
                ...AssignIndividualPermissionProps(dispatch),
                ...UserAuthoritySettingProps(dispatch),
                ...AssignCurrencyProps(dispatch),
                ...ResetPasswordProps(dispatch),
                ...PopboxProps(dispatch)
            })
}