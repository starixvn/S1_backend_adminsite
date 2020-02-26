import React, {Component} from 'react'
import {DropdownBox} from '../../../component/dropdownBox'
import {InputBox} from '../../../component/inputField'
import {CheckList} from '../../../component/checkBox'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'
import { BtnBar } from '../../../component/buttons'
import { Span, SpanClassName } from '../../../component/span'
import {RuleType} from '../../../module/ValidFieldValue'

const ElemType = {
    'Inputbox':'Inputbox',
    'Dropdownbox':'Dropdownbox',
    'Checkbox':'Checkbox',
    'UserIDWithTips':'UserIDWithTips'
}

export function GetAddUserDefineElement(site_options, groups){
    return [
        {
            DataColumn:'userName', DisplayText:Lang.getLang(window.LangCode,"lbl_UserName"), ElemType:ElemType.Inputbox, 
            Rule:{
                [RuleType.NotAllowNull]:
                {
                    AlertMsg:Lang.getLang(window.LangCode,"lbl_UserName") + Lang.getLang(window.LangCode, "msg_not_allow_null")
                },
                [RuleType.MaxLength]:
                {
                    Value:20, 
                    AlertMsg:Lang.getLang(window.LangCode, 'lbl_UserName') + Lang.getLang(window.LangCode, 'msg_please_less_than') + 20 + Lang.getLang(window.LangCode, 'msg_characters')
                }
            }
        },
        {
            DataColumn:'userPWD', DisplayText:Lang.getLang(window.LangCode,"lbl_UserPWD"), ElemType:ElemType.Inputbox, 
            Rule:{
                [RuleType.NotAllowNull]:
                {
                    AlertMsg:Lang.getLang(window.LangCode,"lbl_UserPWD") + Lang.getLang(window.LangCode, "msg_not_allow_null")
                },
                [RuleType.MinLength]:
                {
                    Value:6, 
                    AlertMsg:Lang.getLang(window.LangCode, 'lbl_UserPWD') + Lang.getLang(window.LangCode, 'msg_please_more_than') + 6 + Lang.getLang(window.LangCode, 'msg_characters')
                }, 
                [RuleType.MaxLength]:
                {
                    Value:20, 
                    AlertMsg:Lang.getLang(window.LangCode, 'lbl_UserPWD') + Lang.getLang(window.LangCode, 'msg_please_less_than') + 20 + Lang.getLang(window.LangCode, 'msg_characters')
                }, 
                [RuleType.Regex]:
                {
                    Value:/^[a-zA-Z0-9_.,#?!@$%^&*-]*$/, 
                    AlertMsg:Lang.getLang(window.LangCode, 'msg_UserPWD_format')
                }
            }
        },
        {
            DataColumn:'sites', DisplayText:Lang.getLang(window.LangCode,"lbl_site"), ElemType:ElemType.Checkbox, Options:site_options
        }
    ]
}

export const AddUserPopboxContent = (props)=>{
    return(
        <div className="popup__body">
            <Content
                Datas={props.AddUserDataModel}
                EditField={props.EditField}
                SiteList={props.SiteList}
                GroupList={props.GroupList}
            />
            <div className="popup__row">
                <BtnBar
                    SubmitDisplayText={Lang.getLang(window.LangCode,"lbl_Submit")}
                    CancelDisplayText={Lang.getLang(window.LangCode,"lbl_Cancel")}
                    SuccessAlert={Lang.getLang(window.LangCode,"msgSuccessAlert")}
                    ErrorAlert={Lang.getLang(window.LangCode,"msgErrorAlert")} 
                    Close={props.ClosePopbox}
                    CloseTips={props.CloseTips}
                    ClickSubmit={props.ClickSubmit}
                    ExecutingStatus={props.ExecutingStatus}
                />
            </div>
        </div>
    )    
}

const SetGroupList = (GroupList)=>{
    return GroupList && GroupList.map(x=>{
        return {
            SEQ: x.SEQ,
            DisplayText: x.GroupName
        }
    })
}

const Content = (props)=>{
    let AddUserModel = GetAddUserDefineElement(props.SiteList, SetGroupList(props.GroupList));
    
    let Datas = props.Datas;
    const Content = AddUserModel.map(colInfo=>{
        let key = 'add_user_'+colInfo.DataColumn;
        let _content = null;
        switch(colInfo.ElemType){
            case ElemType.Dropdownbox:
                let options = colInfo.Options;
                _content = (
                        <DropdownBox 
                            Items={options} 
                            OnSelectItem={props.EditField} 
                            SelectValue={Datas[colInfo.DataColumn]}
                            DataColumn={colInfo.DataColumn}
                            HasTitle={false}
                        />
                )
                break;
                
            case ElemType.Checkbox:
                _content = <CheckList 
                    Options={colInfo.Options.map(x=>{return{DisplayText:x.displayText, Value:x.id}})}
                    SelectedDatas={Datas[colInfo.DataColumn]}
                    DisplayColName={Lang.getLang(window.LangCode, "lbl_site")}
                    OnChange={props.EditField}
                    BindingValue={colInfo.DataColumn}
                />
                break;

            default:
                _content = (
                    <InputBox 
                        MainKey={key} 
                        OnBlur={props.EditField}
                        DataColumn={colInfo.DataColumn}
                        Value={Datas[colInfo.DataColumn]}
                        Rule={colInfo.Rule}
                    />
                )
                break;
            };
        
        return (
            <div className="form__group" key={key}>
                <div className="form__title">
                    <span className="text">{colInfo.DisplayText}</span>
                </div>
                {_content}
            </div>
        );
    });
    
    return (
        <div className="popup__row">
            <div className="form">
                {Content}
            </div>
        </div> 
    )
}