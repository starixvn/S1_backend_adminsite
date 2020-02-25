import React from 'react'
import {InputBox} from '../../../component/inputField'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'
import { BtnBar } from '../../../component/buttons'

export const ResetPasswordContent = (props)=>{
    return(
        <div className="popup__body">
            <Content
                Value={props.Value}
                EditField={props.EditField}                    
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
    );
}

const Content = (props)=>{
    return(
        <div className="popup__row">
            <div className="form form--min">
                <div className="form__title">
                    <span className="text">{Lang.getLang(window.LangCode,"lbl_new_password")}</span>
                </div>
                <InputBox 
                    OnBlur={props.EditField}
                    Value={props.Value}
                />
            </div>
        </div> 
    );
}