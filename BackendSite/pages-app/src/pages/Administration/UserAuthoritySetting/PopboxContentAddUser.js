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

export function GetAddUserDefineElement(currencies_options, groups){
    return [
        {
            DataColumn:'GroupName', DisplayText:Lang.getLang(window.LangCode,"lbl_group_name"), ElemType:ElemType.Dropdownbox, Options:groups,
            Rule:{
                [RuleType.NotAllowNull]:
                {
                    AlertMsg:Lang.getLang(window.LangCode,"lbl_group_name") + Lang.getLang(window.LangCode, "msg_not_allow_null")
                }
            }
        },
        {
            DataColumn:'UserID', DisplayText:Lang.getLang(window.LangCode,"lbl_UserID"), ElemType:ElemType.UserIDWithTips, 
            Rule:{
                [RuleType.NotAllowNull]:
                {
                    AlertMsg:Lang.getLang(window.LangCode,"lbl_UserID") + Lang.getLang(window.LangCode, "msg_not_allow_null")
                },
                [RuleType.MaxLength]:
                {
                    Value:20,
                    AlertMsg:Lang.getLang(window.LangCode, 'lbl_UserID') + Lang.getLang(window.LangCode, 'msg_please_less_than') + 20 + Lang.getLang(window.LangCode, 'msg_characters')
                }, 
                [RuleType.Regex]:
                {
                    Value:/^[-()_/0-9a-zA-Z]+$/, 
                    AlertMsg:Lang.getLang(window.LangCode, 'msg_UserID_format')
                },
                [RuleType.NotAllowWhiteSpace]:{
                    AlertMsg:Lang.getLang(window.LangCode, "lbl_UserID") + Lang.getLang(window.LangCode, "msg_not_allow_whitespace")
                }
            }
        },
        {
            DataColumn:'UserName', DisplayText:Lang.getLang(window.LangCode,"lbl_UserName"), ElemType:ElemType.Inputbox, 
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
            DataColumn:'UserPWD', DisplayText:Lang.getLang(window.LangCode,"lbl_UserPWD"), ElemType:ElemType.Inputbox, 
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
            DataColumn:'Currency', DisplayText:Lang.getLang(window.LangCode,"lbl_currency"), ElemType:ElemType.Checkbox, Options:currencies_options
        }
    ]
}

export const AddUserPopboxContent = (props)=>{
    return(
        <div className="popup__body">
            <Content
                Datas={props.AddUserDataModel}
                EditField={props.EditField}
                CurrencyList={props.CurrencyList}
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
    let AddUserModel = GetAddUserDefineElement(props.CurrencyList, SetGroupList(props.GroupList));
    
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
                    Options={colInfo.Options}
                    SelectedDatas={Datas[colInfo.DataColumn]}
                    DisplayColName={Lang.getLang(window.LangCode, "lbl_currency")}
                    OnChange={props.EditField}
                    BindingValue={colInfo.DataColumn}
                />
                break;

            case ElemType.UserIDWithTips:                    
                if(colInfo.PrefixText && Datas[colInfo.PrefixText.DataColumn] 
                    && colInfo.PrefixText.DisplayWhen && Datas[colInfo.PrefixText.DisplayWhen.DataColumn] === colInfo.PrefixText.DisplayWhen.WhenDataColumnValueIs){
                    let _prefixText = Datas[colInfo.PrefixText.DataColumn]
                    _content = (
                        <InputFiedWithPrefixAndTips 
                            IsShow={true}
                            OnBlur={props.EditField}
                            DataColumn={colInfo.DataColumn}
                            Value={Datas[colInfo.DataColumn]} 
                            Rule={colInfo.Rule}
                            PrefixText={_prefixText}
                        />
                    )
                }else{
                    _content = (
                        <InputBox 
                            MainKey={key} 
                            OnBlur={props.EditField}
                            DataColumn={colInfo.DataColumn}
                            Value={Datas[colInfo.DataColumn]}
                            Rule={colInfo.Rule}
                        />
                    )
                }

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

class InputFiedWithPrefixAndTips extends Component{
    constructor(props){
        super(props)
        this.state = {v:this.props.Value, execBlur:false}
    }
    static getDerivedStateFromProps(props, state){
        return state.execBlur && props.Value !== state.v ? {
            ...state,
            v: props.Value
          } : null
    }
    OnChange=(e)=>{
        this.setState({v:e.target.value})
    }
    OnBlur=(e)=>{
        if(this.props.DataColumn){
            this.props.OnBlur(e.target.value, this.props.DataColumn)
        }else{
            this.props.OnBlur(e)
        }
        let checkedValue = this.CheckValue(e.target.value);
        if(!checkedValue.Checked){
            alert(checkedValue.AlertMsg);
        }

        this.setState({execBlur:true})
    }
    OnClick=()=>{
        this.setState({execBlur:false})
    }
    CheckValue=(v)=>{
        let result = { Checked:true, AlertMsg:'Wrong format.' };
        if(v && this.props.Rule){
            let rule = this.props.Rule;
            let length = this.props.PrefixText ? this.props.PrefixText.length + v.length : v.length;
            if(rule.MaxLength && rule.MaxLength.Value < length){
                result.Checked = false;
                result.AlertMsg = rule.MaxLength.AlertMsg
            }
            else if(rule.MinLength && rule.MinLength.Value > length){
                result.Checked = false;
                result.AlertMsg = rule.MinLength.AlertMsg
            }
            else if(rule.Regex && !v.match(rule.Regex.Value)){
                result.Checked = false;
                result.AlertMsg = rule.Regex.AlertMsg
            }
        }

        return result;
    }
    render(){
        let isShow = this.props.IsShow ? ' is-show' : null
        let newUserID = `${this.props.PrefixText}${this.state.v}`
        return(
            <div className={`textfield`}>
                <Span SpanClass={[SpanClassName.textPrimary]} DisplayText={this.props.PrefixText} />
                <input type="text" className="textfield__input" onClick={this.OnClick} value={this.state.v} onChange={this.OnChange} onBlur={this.OnBlur}/>
                <div className={`textfield__tips${isShow}`}>
                    <i className="icon icon-account text-primary"></i>
                    <span className={`${SpanClassName.default}${SpanClassName.textPrimary}`}>
                        {Lang.getLang(window.LangCode,"msg_SubUserID")} <Span SpanClass={[SpanClassName.textHighlight, SpanClassName.textBold]} DisplayText={newUserID}/>
                    </span>
                </div>
            </div>
        )
    }
}