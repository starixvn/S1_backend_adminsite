import React, { useState } from "react";
import { api } from '../../../module/WebAPI';

export const PopBoxContent = (props) => {
    const [password, setpassword] = useState("");
    const Submit = ()=>{
        let Data = {
            PassWord: password,
            SiteId: props.SiteId
          };
        api.post('/api/Payment/CheckPassWord', Data)
                        .then(function (response) {
                            let result = response.data;
                            if (result.errorCode == 0) {
                                props.CheckPassWordSuccess();
                            } else {
                                alert('Password Error');
                            }
                        }).catch(function (error) {
                            alert(error);
                        })
    }
    return (
        <div className="newwrapper">
            <div className="popup-wrap is-show">
                <div className="popup is-open">
                    <div className="popup__head">
                        <span className="text">Please kyin password to confirm the amount </span>
                        {/* <span className="text">{currency} {amount}</span> */}
                        <div className="newbtn"><i className="icon icon-close"></i></div>
                    </div>
                    <div className="popup__body os-host os-theme-dark os-host-resize-disabled os-host-scrollbar-horizontal-hidden os-host-scrollbar-vertical-hidden os-host-transition">
                        <div className="os-resize-observer-host">
                            <div className="os-resize-observer observed"></div>
                        </div>
                        <div className="os-size-auto-observer">
                            <div className="os-resize-observer observed"></div>
                        </div>
                        <div className="os-content-glue"></div>
                        <div className="os-padding">
                            <div className="os-viewport os-viewport-native-scrollbars-invisible">
                                <div className="os-content">
                                    <div className="popup__row">
                                        <div className="form">
                                            <div className="form__group">
                                                <div className="form__title">
                                                    <span className="text">password</span>
                                                </div>
                                                <div className="textfield">
                                                    <input  defaultValue="" onChange={(e) => { setpassword(e.target.value) }} />
                                                </div>
                                            </div>                                                  
                                        </div>
                                    </div>
                                    <div className="popup__row">
                                        <div className="newbtn-bar">
                                            <div className="newbtn-bar__group">
                                                <div className="newbtn newbtn-primary newbtn-w-min" onClick={Submit}><span className="text">OK</span></div>
                                                <div className="newbtn newbtn-secondary newbtn-w-min" onClick={props.ClickCancel}><span className="text">Cancel</span></div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="os-scrollbar os-scrollbar-horizontal os-scrollbar-unusable">
                            <div className="os-scrollbar-track os-scrollbar-track-off">
                                <div className="os-scrollbar-handle">
                                </div>
                            </div>
                        </div>
                        <div className="os-scrollbar os-scrollbar-vertical os-scrollbar-unusable">
                            <div className="os-scrollbar-track os-scrollbar-track-off">
                                <div className="os-scrollbar-handle">
                                </div>
                            </div>
                        </div>
                        <div className="os-scrollbar-corner"></div>
                    </div>
                </div>
            </div>
        </div>
    )
}