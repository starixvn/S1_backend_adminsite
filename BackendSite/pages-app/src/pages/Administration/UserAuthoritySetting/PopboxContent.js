import React from 'react'
import { reducerPropDefines, reducerActions } from './redux/actionsAndDataProperties'
import {AddUserPopboxContent} from './PopboxContentAddUser'
import {IndividualPermissionPopboxContent} from './PopboxContentIndividualPermission'
import {AssignGroupContent} from './PopboxContentAssignGroup'
import {AssignCurrnecyContent} from './PopboxContentAssignCurrency'
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
                        CurrencyList={props.AddUserPopbox.CurrencyList}
                        EditField={props[reducerActions.AddUserActionTypes.EDIT_FIELD]}
                        GroupList={props.AddUserPopbox.GroupList}
                        />
            break;
        case reducerPropDefines.PopboxNames.AssignIndividualPermission:
            content = props.Datas.UserPermissions ? <IndividualPermissionPopboxContent
                        ResetToGroupSetting={props[reducerActions.AssignIndividualPermissionActionTypes.RESET_TO_GROUP_SETTING]} 
                        SaveUserPermission={props[reducerActions.AssignIndividualPermissionActionTypes.ASSIGN_USER_PERMISSION]} 
                        ClosePopbox={props[reducerActions.AssignIndividualPermissionActionTypes.CLOSE_POPUP_ASSIGN_USER_PERMISSION]}
                        Datas={props.Datas.UserPermissions}
                        EnableOrDisableFuncItem={props[reducerActions.AssignIndividualPermissionActionTypes.ENABLE_OR_DISABLE_FUNCTIONITEM]}
                        EnableOrDisableSubOptionsItem={props[reducerActions.AssignIndividualPermissionActionTypes.ENABLE_OR_DISABLE_SUBOPTION]}
                        OpenOrCloseList={props[reducerActions.AssignIndividualPermissionActionTypes.OPEN_OR_CLOSE_MAIN_LIST]}
                        /> : null
            break;
        case reducerPropDefines.PopboxNames.AssignGroup:
            content = <AssignGroupContent
                        ClickSubmit={props[reducerActions.AssignGroupActionTypes.ASSIGN_GROUP]} 
                        ClosePopbox={props[reducerActions.AssignGroupActionTypes.CLOSE_POPUP_ASSIGN_GROUP]}
                        CloseTips={props[reducerActions.PopboxActionTypes.CLOSE_TIPS]}
                        ExecutingStatus={props.PopboxInfo.ExecutingStatus}                   
                        
                        GroupList={props.AssignGroupPopbox.GroupList}
                        CurrentGroupSEQ={props.AssignGroupPopbox.CurrentGroupSEQ}
                        OnSelect={props[reducerActions.AssignGroupActionTypes.ON_SELECT_GROUP]}
                        />
            break;
        case reducerPropDefines.PopboxNames.AssignCurrency:
            content = <AssignCurrnecyContent
                        ClickSubmit={props[reducerActions.AssignCurrencyActionTypes.ASSIGN_CURRENCY]} 
                        ClosePopbox={props[reducerActions.AssignCurrencyActionTypes.CLOSE_POPUP_ASSIGN_CURRENCY]}
                        CloseTips={props[reducerActions.PopboxActionTypes.CLOSE_TIPS]}
                        ExecutingStatus={props.PopboxInfo.ExecutingStatus}                   
                        
                        SelectedValue={props.AssignCurrencyPopbox.SelectedCurr}
                        DataList={props.AssignCurrencyPopbox.CurrencyList}
                        OnSelect={props[reducerActions.AssignCurrencyActionTypes.ON_SELECT_CURRENCY]}
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

