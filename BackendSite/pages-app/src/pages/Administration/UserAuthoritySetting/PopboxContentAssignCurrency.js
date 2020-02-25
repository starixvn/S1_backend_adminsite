import React, { Component } from 'react'
import {CheckBox, CheckBoxClassName} from '../../../component/checkBox'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'
import { BtnBar } from '../../../component/buttons'
import $ from 'jquery'

export class AssignCurrnecyContent  extends Component{
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
                    SelectedValue={this.props.SelectedValue}
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
    let rows = props.DataList && props.DataList.map(data=>{
        let checked = props.SelectedValue.indexOf(data.CurrencyId)!==-1;
        return (
            <div className="form__group" key={`AssignCurrency-${data.CurrencyId}`}>
                <CheckBox
                    ClassName={CheckBoxClassName.checkbox_checked}
                    Checked={checked}
                    HandleClick={props.OnSelect}
                    DisplayText={data.CurrencyName}
                    Value={data.CurrencyId}
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