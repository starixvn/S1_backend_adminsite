import React from 'react'
import { reducerPropDefines, reducerActions } from './redux/actionsAndDataProperties'
import {AddUserPopboxContent} from './PopboxContentAddUser'
import {AssignGroupContent} from './PopboxContentAssignGroup'
import {AssignSiteContent} from './PopboxContentAssignSites'
import {ResetPasswordContent} from './PopboxContentResetpwd'

export default (props)=>{
    let popboxName = props.PopboxInfo.PopboxName;
    let content = null;
    switch(popboxName){
        case reducerPropDefines.PopboxNames.AddUser:
            content = <AddUserPopboxContent
                        ClickSubmit={props[reducerActions.AddUserActionTypes.ADD_USER]} 
                        ClosePopbox={props[reducerActions.AddUserActionTypes.CLOSE_ADD_USER_POPBOX]}
                        CloseTips={props[reducerActions.PopboxActionTypes.CLOSE_TIPS]}
                        ExecutingStatus={props.PopboxInfo.ExecutingStatus}                   
                        AddUserDataModel={props.AddUserPopbox.AddUserDataModel}
                        SiteList={props.AddUserPopbox.SiteList}
                        EditField={props[reducerActions.AddUserActionTypes.EDIT_FIELD]}
                        GroupList={props.AddUserPopbox.GroupList}
                        />
            break;
        case reducerPropDefines.PopboxNames.AssignGroup:
            content = <AssignGroupContent
                        ClickSubmit={props[reducerActions.AssignGroupActionTypes.ASSIGN_GROUP]} 
                        ClosePopbox={props[reducerActions.AssignGroupActionTypes.CLOSE_POPUP_ASSIGN_GROUP]}
                        CloseTips={props[reducerActions.PopboxActionTypes.CLOSE_TIPS]}
                        ExecutingStatus={props.PopboxInfo.ExecutingStatus}                   
                        GroupList={props.AssignGroupPopbox.GroupList}
                        CurrentSelected={props.AssignGroupPopbox.CurrentSelected}
                        OnSelect={props[reducerActions.AssignGroupActionTypes.ON_SELECT_GROUP]}
                        />
            break;
        case reducerPropDefines.PopboxNames.AssignSites:
            content = <AssignSiteContent
                        ClickSubmit={props[reducerActions.AssignSitesActionTypes.ASSIGN_SITES]} 
                        ClosePopbox={props[reducerActions.AssignSitesActionTypes.CLOSE_POPUP_ASSIGN_SITES]}
                        CloseTips={props[reducerActions.PopboxActionTypes.CLOSE_TIPS]}
                        ExecutingStatus={props.PopboxInfo.ExecutingStatus}
                        DataList={props.AssignSitesPopbox.SiteList}
                        OnSelect={props[reducerActions.AssignSitesActionTypes.ON_SELECT_SITES]}
                        />
            break;
        case reducerPropDefines.PopboxNames.ResetPassword:
            content = <ResetPasswordContent
                    ClickSubmit={props[reducerActions.ResetPasswordActionTypes.RESET_PASSWORD]} 
                    ClosePopbox={props[reducerActions.ResetPasswordActionTypes.CLOSE_POPUP_RESET_PASSWORD]}
                    CloseTips={props[reducerActions.PopboxActionTypes.CLOSE_TIPS]}
                    ExecutingStatus={props.PopboxInfo.ExecutingStatus}                   
                    
                    Value={props.ResetPasswordPopbox.NewPassword}
                    EditField={props[reducerActions.ResetPasswordActionTypes.EDIT_RESET_PWD_FIELD]}
                    />
            break;
        default:
            content = <div className="popup__body"></div>
    }

    return content;
}

