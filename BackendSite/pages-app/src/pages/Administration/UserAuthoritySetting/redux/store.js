import {createStore} from 'redux'
import Reducer, {DefineHandlers} from './reducer'
import {reducerPropDefines, reducerActions, webAPIUrl} from './actionsAndDataProperties'
import { composeWithDevTools } from 'redux-devtools-extension'
import WebAPI from '../../../../module/WebAPI'
import Lang from '../../../../resource/Administration/AuthoritySetting/LangAPI'
import {GetAddUserDefineElement} from '../PopboxContentAddUser'
import {VaildFieldValues} from '../../../../module/ValidFieldValue';
import qs from 'qs';
import axios from 'axios';

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
            });

            let paramGetGroupList = {
                Url: webAPIUrl.GET_ALL_GROUP_LIST
            }

            let paramGetSiteList = {
                Url: webAPIUrl.GET_ALL_SITE_LIST
            };

            WebAPI.MultiPostData([paramGetGroupList, paramGetSiteList])
            .then(axios.spread((resGroup, resSites)=> {
                if (resGroup.data.errorCode === 0) {
                    let groupList = resGroup.data.data;
                    dispatch({
                        handler:DefineHandlers.AddUserHandler,
                        type:reducerActions.AddUserActionTypes.UPDATE_GROUP_LIST,
                        groupList:groupList
                    });
                } else {
                    alert(resGroup.data.message)
                }

                if (resSites.data.errorCode === 0) {
                    let siteList = resSites.data.data;
                    dispatch({
                        handler:DefineHandlers.AddUserHandler,
                        type:reducerActions.AddUserActionTypes.UPDATE_SITE_LIST,
                        siteList:siteList
                    });
                } else {
                    alert(resSites.data.message)
                }
            }));
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
                    case reducerPropDefines.AddUserDataModelProperty.sites:
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
            let checked = VaildFieldValues(newUser, defineElems);

            if(checked){
                let param = {
                    Url: webAPIUrl.ADD_USER,
                    Data: qs.stringify({
                        UserName:newUser.userName,
                        Userpwd:newUser.userPWD,                        
                        SiteList:newUser.sites,
                        GroupSEQ:newUser.group ? newUser.group.Id : null
                    })
                };

                WebAPI.PostData(param).then((res) => {
                    if (res.data.errorCode === 0) {
                        dispatch({
                            handler:DefineHandlers.AddUserHandler,
                            type:reducerActions.AddUserActionTypes.ADD_USER,
                            datas:res.data.data
                        })
                    } else {
                        alert(res.data.message ?? 'failed to get data.');
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

            WebAPI.PostData(param).then((res)=> {
                if (res.data.errorCode === 0) {
                    dispatch({
                        handler:DefineHandlers.UserAuthoritySettingHandler,
                        type:reducerActions.UserAuthoritySettingActionTypes.GET_USER_LIST,
                        datas:res.data.data
                    })
                } else {
                    alert(res.data.message ?? 'failed to get data.');
                }
            })
        },
        [reducerActions.UserAuthoritySettingActionTypes.CLICK_ENABLE_USER]:(v, idx)=>{
            let datas = store.getState().CommonHandler.Datas.UserList;
            let status = v === 'false' ? 1 : 0;

            let param = {
                Url: webAPIUrl.CLICK_ENABLE_USER,
                Data: qs.stringify({
                    UserId:datas[idx].userId,
                    Status: status
                })
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    datas[idx].status = status;
                    dispatch({
                        handler:DefineHandlers.UserAuthoritySettingHandler,
                        type:reducerActions.UserAuthoritySettingActionTypes.CLICK_ENABLE_USER,
                        UserList:datas
                    })
                } else {
                    alert(res.data.message ?? 'failed to get data.');
                }
            })
        }
    }
}

function GetTitleDisplayText(PopNameLangKey, userName){
    return `${Lang.getLang(window.LangCode, PopNameLangKey)} - ${Lang.getLang(window.LangCode,"lbl_UserName")}：${userName}`;
}

function ResetPasswordProps(dispatch){
    return {
        [reducerActions.ResetPasswordActionTypes.OPEN_POPUP_RESET_PASSWORD]:(tableName, idx)=>{
            let userList = store.getState().CommonHandler.Datas.UserList;
            dispatch({
                handler:DefineHandlers.ResetPasswordHandler,
                type:reducerActions.ResetPasswordActionTypes.OPEN_POPUP_RESET_PASSWORD,
                title:GetTitleDisplayText("lbl_change_password", userList[idx].userName),
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
                    Data: qs.stringify({
                        Password:newPassword,
                        UserId:userList[_identityKey.idx].userId
                    })
                };

                WebAPI.PostData(param).then((res) => {
                    if (res.data.errorCode === 0) {
                        dispatch({
                            handler:DefineHandlers.ResetPasswordHandler,
                            type:reducerActions.ResetPasswordActionTypes.RESET_PASSWORD,
                        })
                    } else {
                        alert(res.data.message ?? 'failed to get data.');
                    }
                });
            }
        }
    }
}

function AssignSitesProps(dispatch){
    return {
        [reducerActions.AssignSitesActionTypes.OPEN_POPUP_ASSIGN_SITES]:(tableName, idx)=>{
            let datas = store.getState().CommonHandler.Datas.UserList;
            let param = {
                Url: webAPIUrl.GET_SITE_LIST,
                Data: qs.stringify({
                    UserId:datas[idx].userId
                })
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    let siteList = res.data.data;
                    let title = GetTitleDisplayText("lbl_site", datas[idx].userName); 
                    dispatch({
                        handler:DefineHandlers.AssignSitesHandler,
                        type:reducerActions.AssignSitesActionTypes.OPEN_POPUP_ASSIGN_SITES,
                        title:title,
                        popboxName:reducerPropDefines.PopboxNames.AssignSites,
                        siteList:siteList,
                        identityKey:{idx:idx}
                    });
                } else {
                    alert(res.data.message ?? 'failed to get data.');
                }
            })
        },
        [reducerActions.AssignSitesActionTypes.CLOSE_POPUP_ASSIGN_SITES]:()=>{
            dispatch({
                handler:DefineHandlers.AssignSitesHandler,
                type:reducerActions.AssignSitesActionTypes.CLOSE_POPUP_ASSIGN_SITES
            })
        },
        [reducerActions.AssignSitesActionTypes.ON_SELECT_SITES]:(idx)=>{
            let datas = store.getState().CommonHandler.AssignSitesPopbox.SiteList;
            datas[idx].selected = !datas[idx].selected;

            dispatch({
                handler:DefineHandlers.AssignSitesHandler,
                type:reducerActions.AssignSitesActionTypes.ON_SELECT_SITES,
                siteList:datas
            });
        },
        [reducerActions.AssignSitesActionTypes.ASSIGN_SITES]:()=>{
            if(window.confirm(Lang.getLang(window.LangCode,"msg_save_changes"))){
                let userList = store.getState().CommonHandler.Datas.UserList;
                let _identityKey = store.getState().CommonHandler.PopboxInfo.IdentityKey;
                
                let selectedSiteIds = store.getState().CommonHandler.AssignSitesPopbox.SiteList.filter(s=>{return s.selected === true}).map(s=>{return s.id});
                let param = {
                    Url: webAPIUrl.ASSIGN_SITES,
                    Data: qs.stringify({
                        UserId:userList[_identityKey.idx].userId,
                        SiteIdList:selectedSiteIds
                    })
                };

                WebAPI.PostData(param).then((res) => {
                    if (res.data.errorCode === 0) {
                        dispatch({
                            handler:DefineHandlers.AssignSitesHandler,
                            type:reducerActions.AssignSitesActionTypes.ASSIGN_SITES,
                        })
                    } else {
                        alert(res.data.message ?? 'failed to get data.');
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
                Url: webAPIUrl.GET_ALL_GROUP_LIST
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    let title = GetTitleDisplayText("lbl_role", datas[idx].userName);
                    let groupList = res.data.data;
                    dispatch({
                        handler:DefineHandlers.AssignGroupHandler,
                        type:reducerActions.AssignGroupActionTypes.OPEN_POPUP_ASSIGN_GROUP,
                        title:title,
                        popboxName:reducerPropDefines.PopboxNames.AssignGroup,
                        groupList:groupList,
                        identityKey:{idx:idx},
                        currSelectedId:datas[idx].groupSEQ
                    })
                } else {
                    alert(res.data.message ?? 'failed to get data.');
                }
            })
        },
        [reducerActions.AssignGroupActionTypes.CLOSE_POPUP_ASSIGN_GROUP]:()=>{
            dispatch({
                handler:DefineHandlers.AssignGroupHandler,
                type:reducerActions.AssignGroupActionTypes.CLOSE_POPUP_ASSIGN_GROUP
            })
        },
        [reducerActions.AssignGroupActionTypes.ON_SELECT_GROUP]:(v)=>{
            dispatch({
                handler:DefineHandlers.AssignGroupHandler,
                type:reducerActions.AssignGroupActionTypes.ON_SELECT_GROUP,
                currSelectedId:v
            })
        },
        [reducerActions.AssignGroupActionTypes.ASSIGN_GROUP]:()=>{
            if(window.confirm(Lang.getLang(window.LangCode,"msg_save_changes"))){
                let new_group = store.getState().CommonHandler.AssignGroupPopbox;
                if(new_group.CurrentSelected===null || new_group.CurrentSelected===undefined){
                    dispatch({
                        handler:DefineHandlers.AssignGroupHandler,
                        type:reducerActions.AssignGroupActionTypes.CLOSE_POPUP_ASSIGN_GROUP
                    })
                }else{
                    let datas = store.getState().CommonHandler.Datas.UserList;
                    let _identityKey = store.getState().CommonHandler.PopboxInfo.IdentityKey;

                    let param = {
                        Url: webAPIUrl.ASSIGN_GROUP,
                        Data: qs.stringify({
                            GroupSEQ:new_group.CurrentSelected,
                            UserId:datas[_identityKey.idx].userId
                        })
                    };

                    WebAPI.PostData(param).then((res) => {
                        if (res.data.errorCode === 0) {
                            let groupInfo = new_group.GroupList.find(x=>x.id===Number(new_group.CurrentSelected));
                            datas[_identityKey.idx].groupName = groupInfo === undefined ? null : groupInfo.displayText;
                            datas[_identityKey.idx].groupSEQ = groupInfo === undefined ? null : groupInfo.id;

                            dispatch({
                                handler:DefineHandlers.AssignGroupHandler,
                                type:reducerActions.AssignGroupActionTypes.ASSIGN_GROUP,
                                UserList:datas
                            })
                        } else {
                            alert(res.data.message ?? 'failed to get data.');
                        }
                    })
                }
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
                ...UserAuthoritySettingProps(dispatch),
                ...AssignSitesProps(dispatch),
                ...ResetPasswordProps(dispatch),
                ...PopboxProps(dispatch)
            })
}