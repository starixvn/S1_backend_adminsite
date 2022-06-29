import React, { useState } from "react";
import { api } from '../../../module/WebAPI';
export const PopBoxContentAddLevel = (props) => {
    let lastList = props.LevelList.length === 0 ? [] : props.LevelList[props.LevelList.length - 1];
    let siteId = props.SiteId, depositType = props.DepositType, currencyId = props.CurrencyId, vipLevel = 1, depositMin = 1, depositMax = 1, depositStatus = 1;

    if (lastList.length !== 0) {
        vipLevel = parseInt(lastList.vipLevel) + 1;
        // depositMin = parseInt(lastList.depositMin) + 1;
        // depositMax = parseInt(lastList.depositMax) + 1;
        depositMin = 0
        depositMax = 0
    }

    const [Level, setLevel] = useState(vipLevel);
    const [DepositMin, setDepositMin] = useState(depositMin);
    const [DepositMax, setDepositMax] = useState(depositMax);
    let newLevelList = { siteId: siteId, depositType: depositType, currencyId: currencyId, vipLevel: Level, depositMin: DepositMin, depositMax: DepositMax, depositStatus: depositStatus }
    const handleSave = (e) => {
        e.preventDefault();

        if (lastList.vipLevel >= Level) { return (alert("err1")) };
        // if (lastList.depositMin >= DepositMin) { return (alert("Deposit Min too small")) };
        // if (lastList.depositMax >= DepositMax) { return (alert("Deposit Max too small")) };
        var data = {
            siteId: siteId,
            currencyId: currencyId,
            after:{
                depositType: depositType,
                vipLevel: Level,
                depositMin: DepositMin,
                depositMax: DepositMax,
                depositStatus: depositStatus
            }
        }
        api.post('/apiv2/DepositLimit/Create', data)
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    props.ClickSave(newLevelList);
                    alert(response.data.message);
                } else {
                    alert(response.data.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }

    return (
        <div className="newwrapper">
            <div className="popup-wrap is-show">
                <div className="popup is-open">
                    <div className="popup__head"><span className="text">Add new Vip level</span>
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
                                                    <span className="text">Vip Level</span>
                                                </div>
                                                <div className="textfield">
                                                    <input readOnly defaultValue={vipLevel} onChange={(e) => { setLevel(parseInt(e.target.value)) }} />
                                                </div>
                                            </div>
                                            <div className="form__group">
                                                <div className="form__title">
                                                    <span className="text">Deposit Min</span>
                                                </div>
                                                <div className="textfield">
                                                    <input type="number" defaultValue={depositMin} onChange={(e) => { setDepositMin(parseInt(e.target.value)) }} />
                                                </div>
                                            </div>
                                            <div className="form__group">
                                                <div className="form__title">
                                                    <span className="text">Deposit Max</span>
                                                </div>
                                                <div className="textfield">
                                                    <input type="number" defaultValue={depositMax} onChange={(e) => { setDepositMax(parseInt(e.target.value)) }} />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="popup__row">
                                        <div className="newbtn-bar">
                                            <div className="newbtn-bar__group">
                                                <div className="newbtn newbtn-primary newbtn-w-min" onClick={handleSave}><span className="text">Save</span></div>
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