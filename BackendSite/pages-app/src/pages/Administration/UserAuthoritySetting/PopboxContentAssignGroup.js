import React, { Component } from 'react'
import {RadioBox, RadioBoxClassName} from '../../../component/radioBox'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'
import { BtnBar } from '../../../component/buttons'
import $ from 'jquery'

export class AssignGroupContent  extends Component{
    componentDidMount=()=>{
        if(process.env.NODE_ENV === 'production'){
            $(this.refs.popup_body).overlayScrollbars({});
        }
    }
    render(){
        return(
            <div className="popup__body" ref="popup_body">
                <Content
                    GroupList={this.props.GroupList}
                    CurrentGroupSEQ={this.props.CurrentGroupSEQ}
                    OnSelect={this.props.OnSelect}                    
                />
                <div className="popup__row">
                    <BtnBar
                        SubmitDisplayText={Lang.getLang(window.LangCode,"lbl_Submit")}
                        CancelDisplayText={Lang.getLang(window.LangCode,"lbl_Cancel")}
                        SuccessAlert={Lang.getLang(window.LangCode,"msgSuccessAlert")}
                        ErrorAlert={Lang.getLang(window.LangCode,"msgErrorAlert")} 
                        Close={this.props.ClosePopbox}
                        CloseTips={this.props.CloseTips}
                        ClickSubmit={this.props.ClickSubmit}
                        ExecutingStatus={this.props.ExecutingStatus}
                    />
                </div>
            </div>
        )
    }
}

const Content = (props)=>{
    let radioGroup = 'AssignGroup'
    let rows = props.GroupList && props.GroupList.map(group=>{
        let checked = props.CurrentGroupSEQ===group.SEQ
        return (
            <div className="form__group" key={`AssignGroup-${group.SEQ}`}>
                <RadioBox
                    ClassName={RadioBoxClassName.checkbox_checked}
                    RadioGroup={radioGroup}
                    DisplayText={group.GroupName}
                    Value={group.SEQ}
                    Checked={checked}
                    BindingValue={group.GroupName}
                    HandleClick={props.OnSelect}
                />
            </div>
        );
    });

    return(
        <div className="popup__row">
            <div className="form">
                {rows}
            </div>
        </div> 
    );
}