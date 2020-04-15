import React, { Component } from 'react'
import {CheckBox, CheckBoxClassName} from '../../../component/LAP/checkBox'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'
import { BtnBar } from '../../../component/LAP/buttons'
import $ from 'jquery'

export class AssignSiteContent extends Component{
    componentDidMount=()=>{
        if(process.env.NODE_ENV === 'production'){
            $(this.refs.popup_body).overlayScrollbars({});
        }
    }
    render(){
        return(
            <div className="popup__body" ref="popup_body">
                <Content
                    DataList={this.props.DataList}
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
    let rows = props.DataList && props.DataList.map((data, idx)=>{
        let checked = data.selected;
        return (
            <div className="form__group" key={`AssignSite-${data.id}-${idx}`}>
                <CheckBox
                    ClassName={CheckBoxClassName.checkbox_checked}
                    Checked={checked}
                    HandleClick={props.OnSelect}
                    DisplayText={data.displayText}
                    Value={idx}
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