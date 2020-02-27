import React, { Component } from 'react'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'
import {DropdownBox} from '../../../component/dropdownBox'
import {InputBox} from '../../../component/inputField'
import { BtnBar } from '../../../component/buttons'
import {RuleType} from '../../../module/ValidFieldValue'
import Permissions from '../component/permissionList'

export default class Panel extends Component {
    render() {
        return (
            <div className="panel">
                <GroupInfo
                    IsAdd={this.props.GroupInfo.IsAdd}
                    Datas={this.props.GroupInfo.Datas}
                    EditField={this.props.GroupInfo.EditField}
                />
                <Permissions
                    Datas={this.props.Datas.PermissionDatas}
                    Functions={
                        {
                            HandleList:this.props.Datas.HandleList,
                            EnableOrDisableFuncItem:this.props.EnableOrDisableFuncItem,
                            EnableOrDisableSubOptionsItem:this.props.EnableOrDisableSubOptionsItem,
                            OpenOrCloseList:this.props.OpenOrCloseList
                        }
                    }
                    IsUserAuthPage={false}
                    IsFullPage={true}
                />
                <div className="panel__row">
                    <BtnBar
                        SubmitDisplayText={Lang.getLang(window.LangCode,"lbl_Submit")}
                        CancelDisplayText={Lang.getLang(window.LangCode,"lbl_Cancel")}
                        Close={this.props.BtnBar.ClickCancel}
                        ClickSubmit={this.props.BtnBar.ClickSubmit}
                    />
                </div>
            </div>
        )
    }
}

const ElemType = {
    'Inputbox':'Inputbox',
    'Dropdownbox':'Dropdownbox'
}

export function GetGroupInfoDefineElement(IsAdd){
    return [
        {
            DataColumn:'groupName', DisplayText:Lang.getLang(window.LangCode,"lbl_group_name"), ElemType:ElemType.Inputbox, 
            Rule:{
                [RuleType.NotAllowNull]:
                {
                    AlertMsg:Lang.getLang(window.LangCode,"lbl_group_name") + Lang.getLang(window.LangCode, "msg_not_allow_null")
                }
            }
        },
        {
            DataColumn:'remark', DisplayText:Lang.getLang(window.LangCode,"lbl_remark"), ElemType:ElemType.Inputbox
        }
    ] 
}

class GroupInfo extends Component{
    get DefineElements(){
        return GetGroupInfoDefineElement(this.props.IsAdd)
    }
    get Content(){
        let dataModel = this.DefineElements;
        let Datas = this.props.Datas;
        return dataModel.map(colInfo=>{
            let key = 'groupInfo_'+colInfo.DataColumn;
            let _content = null;
            switch(colInfo.ElemType){
                case ElemType.Dropdownbox:
                    let options = this.DropdownboxOptions(colInfo.DataColumn);
                    _content = (
                            <DropdownBox 
                                MainKey={key} 
                                Items={options} 
                                OnSelectItem={this.props.EditField} 
                                SelectValue={Datas[colInfo.DataColumn]}
                                DataColumn={colInfo.DataColumn}
                                HasTitle={false}
                            />
                    )
                    break;

                default:
                    _content = (
                        <InputBox 
                            Key={key} 
                            OnBlur={this.props.EditField}
                            DataColumn={colInfo.DataColumn}
                            Value={Datas[colInfo.DataColumn]}
                            Rule={colInfo.Rule}
                        />
                    )
                    break;
            }
            
            return (
                <div className="form__group" key={key}>
                    <div className="form__title">
                        <span className="text">{colInfo.DisplayText}</span>
                    </div>
                    {_content}
                </div>
            )
        })
    }
    render(){
        return(
            <div className="panel__row">
                <div className="block block-primary">
                    <div className="form">
                        {this.Content}
                    </div>
                </div>
            </div>
        )
    }
}
