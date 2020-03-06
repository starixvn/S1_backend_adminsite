import {createStore} from 'redux'
import Reducer, {DefineHandlers} from './reducer'
import {reducerActions, webAPIUrl, InitDataModel} from './actionsAndDataProperties'
import { composeWithDevTools } from 'redux-devtools-extension'
import WebAPI from '../../../../module/WebAPI'
import Lang from '../../../../resource/Administration/AuthoritySetting/LangAPI'
import {VaildFieldValues} from '../../../../module/ValidFieldValue'
import {GetGroupInfoDefineElement} from '../EditPanel'

export let store = process.env.NODE_ENV==='production' ? createStore(Reducer) : createStore(Reducer, composeWithDevTools());

export function mapStateToProps(state){
    let props = {};
    Object.keys(state.CommonHandler).forEach(key=>{
        props[key] = state.CommonHandler[key]
    })

    return props;
}

function GetPermissionDataModel(IsEdit, dispatch, WhiteLabel=null){
    let param = {
        Url: webAPIUrl.GetPermissionDatas
    };

    WebAPI.PostData(param).then((res) => {
        if (res.data.errorCode === 0) {
            if(IsEdit){
                dispatch({
                    handler:DefineHandlers.PermissionListHandler,
                    type:reducerActions.PermissionListActionTypes.UPDATE_PERMISSION_MODEL,
                    datas:res.data.data
                })
            }else{
                dispatch({
                    handler:DefineHandlers.GroupAuthorityHandler,
                    type:reducerActions.GroupAuthorityActionTypes.CLICK_ADD_GROUP,
                    actionType:'CLICK_ADD_GROUP',
                    permissionDatas:res.data.data
                })
            }
            window.scrollTo(0, 0)
        } else {
            alert(res.data.message ?? 'failed to get data.');
        }
    })
}

function GroupAuthorityProps(dispatch){
    return {
        [reducerActions.GroupAuthorityActionTypes.GET_GROUP_LIST]:()=>{
            let param = {
                Url: webAPIUrl.GetGroupList
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    dispatch({
                        handler:DefineHandlers.GroupAuthorityHandler,
                        type:reducerActions.GroupAuthorityActionTypes.GET_GROUP_LIST,
                        datas:res.data.data === null ? [] : res.data.data
                    })
                } else {
                    alert(res.data.message ?? 'failed to get data.');
                }
            })
        },
        [reducerActions.GroupAuthorityActionTypes.CLICK_DELETE_GROUP]:(tableName, idx)=>{
            if(window.confirm(Lang.getLang(window.LangCode,"msg_delete_group"))){
                let datas = store.getState().CommonHandler.Datas.GroupList;
                let param = {
                    Url: webAPIUrl.DeleteGroup,
                    Data:{
                        GroupSEQ:datas[idx].groupSEQ
                    }
                };
    
                WebAPI.PostData(param).then((res) => {
                    if (res.data.errorCode === 0) {
                        let InitDatas = store.getState().CommonHandler.Datas.InitGroupList;
                        InitDatas = InitDatas.filter(x=>x.groupSEQ !== datas[idx].groupSEQ);
                        datas.splice(idx, 1);
                        dispatch({
                            handler:DefineHandlers.GroupAuthorityHandler,
                            type:reducerActions.GroupAuthorityActionTypes.CLICK_DELETE_GROUP,
                            datas:datas,
                            initDatas:InitDatas
                        })
                    }else{
                        alert(res.data.message ?? 'failed to get data.');
                    }
                })
            }
        },
        [reducerActions.GroupAuthorityActionTypes.CLOSE_EDIT_PANEL]:()=>{
            dispatch({
                handler:DefineHandlers.GroupAuthorityHandler,
                type:reducerActions.GroupAuthorityActionTypes.CLOSE_EDIT_PANEL
            })
        },
        [reducerActions.GroupAuthorityActionTypes.CLICK_ADD_GROUP]:()=>{
            GetPermissionDataModel(false, dispatch)
        },
        [reducerActions.GroupAuthorityActionTypes.ADD_GROUP]:()=>{
            let groupInfo = store.getState().CommonHandler.Edit.GroupInfo;
            let defineElems = GetGroupInfoDefineElement(true);
            if(VaildFieldValues(groupInfo, defineElems)){
                let data = store.getState().CommonHandler.Datas.Permissions.PermissionDatas;
                let selected = SetSelectedPermissionDatas(data);
                let param = {
                    Url: webAPIUrl.SaveGroup,
                    Data:{
                        GroupName:groupInfo.groupName,
                        Remark:groupInfo.remark,
                        Permission:selected
                    }
                };
                
                WebAPI.PostData(param).then((res) => {
                    if (res.data.errorCode === 0) {
                        dispatch({
                            handler:DefineHandlers.GroupAuthorityHandler,
                            type:reducerActions.GroupAuthorityActionTypes.ADD_GROUP,
                            datas:res.data.data
                        })
                        alert(Lang.getLang(window.LangCode,"msgSuccessAlert"))
                    } else {
                        alert(res.data.message ?? 'failed to get data.');
                    }
                });
            }
        },
        [reducerActions.GroupAuthorityActionTypes.EDIT_GROUP_INFO_FIELD]:(value, dataColumn)=>{
            let datas = store.getState().CommonHandler.Edit.GroupInfo;
            datas[dataColumn] = value;

            dispatch({
                handler:DefineHandlers.GroupAuthorityHandler,
                type:reducerActions.GroupAuthorityActionTypes.EDIT_GROUP_INFO_FIELD,
                groupInfo:datas
            })
        },
        [reducerActions.GroupAuthorityActionTypes.CLICK_EDIT_GROUP]:(tableName, idx)=>{
            let data = store.getState().CommonHandler.Datas.GroupList[idx];
            
            let groupInfo = {}
            Object.keys(InitDataModel.Edit.GroupInfo).forEach(k=>{
                groupInfo[k] = data[k];
            });
            
            let param = {
                Url: webAPIUrl.GetGroupInfo,
                Data:{
                    GroupSEQ:data.groupSEQ
                }
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    dispatch({
                        handler:DefineHandlers.GroupAuthorityHandler,
                        type:reducerActions.GroupAuthorityActionTypes.CLICK_EDIT_GROUP,
                        actionType:'CLICK_EDIT_GROUP',
                        permissionDatas:res.data.data,
                        groupInfo:groupInfo,
                        identityKey:{idx:idx}
                    })
                    window.scrollTo(0, 0)
                } else {
                    alert(res.data.message ?? 'failed to get data.');
                }
            })
        },
        [reducerActions.GroupAuthorityActionTypes.EDIT_GROUP]:()=>{
            let newGroupInfo = store.getState().CommonHandler.Edit.GroupInfo;
            let defineElems = GetGroupInfoDefineElement(false);
            if(VaildFieldValues(newGroupInfo, defineElems)){
                let groupList = store.getState().CommonHandler.Datas.GroupList;
                let identityKey = store.getState().CommonHandler.Edit.IdentityKey;
                let idx = identityKey.idx;

                let data = store.getState().CommonHandler.Datas.Permissions.PermissionDatas;
                let selected = SetSelectedPermissionDatas(data);
                
                let param = {
                    Url: webAPIUrl.UpdateGroup,
                    Data:{
                        GroupSEQ:groupList[idx].groupSEQ,
                        GroupName:newGroupInfo.groupName,
                        Remark:newGroupInfo.remark,
                        Permission:selected
                    }
                };
                
                WebAPI.PostData(param).then((res) => {
                    let idx = identityKey.idx;
                    let initGroupList = store.getState().CommonHandler.Datas.InitGroupList;
                    let getIdx = initGroupList.findIndex(x=>x.groupSEQ===groupList[idx].groupSEQ);
                    initGroupList[getIdx].groupName = newGroupInfo.groupName
                    initGroupList[getIdx].remark = newGroupInfo.remark

                    if (res.data.errorCode === 0) {
                        dispatch({
                            handler:DefineHandlers.GroupAuthorityHandler,
                            type:reducerActions.GroupAuthorityActionTypes.EDIT_GROUP,
                            datas:initGroupList
                        })
                        
                        alert(Lang.getLang(window.LangCode,"msgSuccessAlert"))
                    } else {
                        alert(res.data.message ?? 'failed to get data.');
                    }
                });
            }
        }
    }
}

function SetSelectedPermissionDatas(datas){
    let result = [];
    for(let i in datas){
        let main = datas[i];        
        let selected = SetSelectedFunctionKeys(main.settings);
        if(selected.length !== 0){
            result = result.concat(selected);
        }
    }

    return result;
}

function SetSelectedFunctionKeys(settings){
    let result = [];
    for(let s in settings){
        let setting = settings[s];
        if(setting.enable){
            if(setting.funcKeys){
                let funckeys = SetSelectedFunctionKeys(setting.funcKeys);
                if(funckeys && funckeys.length > 0){
                    result.push({funckey:setting.funcKey, subFuncKeys:null})
                    result = result.concat(funckeys)
                }
            }else{
                let subOptions = [];                
                if(setting.subFuncKeys){
                    setting.groupCheckedIndex && setting.groupCheckedIndex.forEach(idx=>{
                        subOptions.push(setting.subFuncKeys[idx].funcKey)
                    });

                    if(subOptions.length!==0){
                        result.push({funcKey:setting.funcKey, subFuncKeys:subOptions});    
                    }                    
                }else{
                    result.push({funcKey:setting.funcKey, subFuncKeys:null});                    
                }
            }
        }
    }

    return result;
}


function EnableOrDisableAllFunctionKeys(datas, checked, isUserAuthPage){
    for(let i in datas){
        let setting = datas[i]
        if(isUserAuthPage && setting.isGroupSetting && !setting.funcKeys){
            continue;
        }

        setting.enable = setting.isGroupSetting ? setting.enable : checked
        if(isUserAuthPage){
            setting.userCheckedIndex = !checked ? [] : setting.userCheckedIndex
        }else{
            setting.groupCheckedIndex = !checked ? [] : setting.groupCheckedIndex
        }
        if(setting.funcKeys){
            setting.funcKeys = EnableOrDisableAllFunctionKeys(setting.funcKeys, checked, isUserAuthPage);
        }
    }

    return datas
}

function EnableOrDisableFunctionKeys(datas, checked, FuncKey, isUserAuthPage){
    for(let i in datas){
        let setting = datas[i]
        if(isUserAuthPage && setting.isGroupSetting && !setting.funcKeys){
            continue
        }
        
        if(setting.funcKey===FuncKey){
            setting.enable = isUserAuthPage && setting.isGroupSetting ? setting.enable : checked
            setting.funcKeys = !checked && setting.funcKeys ? EnableOrDisableAllFunctionKeys(setting.funcKeys, checked, isUserAuthPage) : setting.funcKeys
            
            if(isUserAuthPage){
                setting.userCheckedIndex = !checked ? [] : setting.userCheckedIndex
            }else{
                setting.groupCheckedIndex = !checked ? [] : setting.groupCheckedIndex
            }
            
            break;
        }else if(setting.funcKeys){
            setting.funcKeys = EnableOrDisableFunctionKeys(setting.funcKeys, checked, FuncKey, isUserAuthPage)
        }
    }

    return datas
}

function EnableOrDisableSubOption(datas, optIdx, FuncKey, isUserAuthPage){
    for(let i in datas){
        let setting = datas[i]
        if(setting.funcKey===FuncKey){
            let targetFiled = isUserAuthPage ? 'userCheckedIndex' : 'groupCheckedIndex'
            if(setting.inputBoxType==='radio' && setting.isGroupSetting && isUserAuthPage) continue;
            setting[targetFiled] = setting[targetFiled] === undefined ? [] : setting[targetFiled];
            let find_idx = setting[targetFiled] && setting[targetFiled].indexOf(optIdx);
            if(find_idx===-1){
                if(setting.inputBoxType==='radio'){
                    setting[targetFiled]=[optIdx]
                }else{
                    setting[targetFiled].push(optIdx)
                }
            }else if(find_idx===null){
                setting[targetFiled] = [optIdx]
            }else{
                if(setting.inputBoxType==='radio'){
                    setting[targetFiled]=[]
                }else{
                    setting[targetFiled].splice(find_idx, 1)
                }
            }
            
            break;
        }else if(setting.funcKeys){
            setting.funcKeys = EnableOrDisableSubOption(setting.funcKeys, optIdx, FuncKey, isUserAuthPage)
        }
    }

    return datas
}


function PermissionListProps(dispatch){
    return {
        [reducerActions.PermissionListActionTypes.OPEN_OR_CLOSE_MAIN_LIST]:(MainIdx)=>{
            let handleList = store.getState().CommonHandler.Datas.Permissions.HandleList;
            let find_idx = handleList.CloseList_MainItemIdx.indexOf(MainIdx)
            if(find_idx===-1){
                handleList.CloseList_MainItemIdx.push(MainIdx)
            }else{
                handleList.CloseList_MainItemIdx.splice(find_idx,1)
            }

            dispatch({
                handler:DefineHandlers.PermissionListHandler,
                type:reducerActions.PermissionListActionTypes.OPEN_OR_CLOSE_MAIN_LIST,
                datas:handleList
            })
        },
        [reducerActions.PermissionListActionTypes.ENABLE_OR_DISABLE_FUNCTIONITEM]:(v, param={FuncKey:null, MainIdx:null, IsTopItem:false})=>{
            let datas = store.getState().CommonHandler.Datas.Permissions.PermissionDatas;
            let isUserAuthPage = false;
            if(param){
                let checked = v==='false';
                let topItemidx = param.MainIdx;
                if(param.IsTopItem){                    
                    datas[topItemidx].enable = checked
                    for(let i in datas[topItemidx].settings){
                        let setting = datas[topItemidx].settings[i]
                        if(isUserAuthPage && setting.isGroupSetting && !setting.funcKeys){
                            continue;
                        }
                        setting.enable = isUserAuthPage && setting.isGroupSetting ? setting.enable : checked
                        setting.funcKeys = setting.funcKeys ? EnableOrDisableAllFunctionKeys(setting.funcKeys, checked, isUserAuthPage) : setting.funcKeys;
                    }
                }else{
                    datas[topItemidx].settings = EnableOrDisableFunctionKeys(datas[topItemidx].settings, checked, param.FuncKey, isUserAuthPage)
                }
            }
            
            dispatch({
                handler:DefineHandlers.PermissionListHandler,
                type:reducerActions.PermissionListActionTypes.ENABLE_OR_DISABLE_FUNCTIONITEM,
                datas:datas
            })
        },
        [reducerActions.PermissionListActionTypes.ENABLE_OR_DISABLE_SUBOPTION]:(v, param={FuncKey:null, MainIdx:null, IsTopItem:false})=>{
            let datas = store.getState().CommonHandler.Datas.Permissions.PermissionDatas;
            let isUserAuthPage = false;
            datas[param.MainIdx].settings = EnableOrDisableSubOption(datas[param.MainIdx].settings, Number(v), param.FuncKey, isUserAuthPage)
            dispatch({
                handler:DefineHandlers.PermissionListHandler,
                type:reducerActions.PermissionListActionTypes.ENABLE_OR_DISABLE_SUBOPTION,
                datas:datas
            })
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
                ...GroupAuthorityProps(dispatch),
                ...PermissionListProps(dispatch),
                ...PopboxProps(dispatch)
            })
}