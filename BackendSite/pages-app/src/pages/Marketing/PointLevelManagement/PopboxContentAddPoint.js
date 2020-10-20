import React, { useState } from "react";
import { api } from '../../../module/WebAPI';
export const PopBoxContentAddPoint = (props) => {
    let lastList = props.BankList.length === 0 ? [] : props.BankList[props.BankList.length - 1];
    let siteId = props.SiteId, currencyId = props.CurrencyId, pointLevel = 2, depositCnt = 1, withdrawalCnt = 1, isKyc = 1;

    if (lastList.length !== 0) {
        pointLevel = parseInt(lastList.pointLevel) + 1;
        depositCnt = parseInt(lastList.depositCnt) + 1;
        withdrawalCnt = parseInt(lastList.withdrawalCnt) + 1;
    }

    const [Point, setPoint] = useState(pointLevel);
    const [DepositCnt, setDepositCnt] = useState(depositCnt);
    const [WithdrawalCnt, setWithdrawalCnt] = useState(withdrawalCnt);
    const [IsKyc, setIsKyc] = useState(isKyc);
    let newBankList = { siteId: siteId, currencyId: currencyId, pointLevel: Point, depositCnt: DepositCnt, withdrawalCnt: WithdrawalCnt, isKyc: IsKyc }
    const handleSave = (e) => {
        e.preventDefault();

        if (lastList.pointLevel >= Point) { return (alert("err1")) };
        // if (lastList.depositCnt >= DepositCnt) { return (alert("Deposit Count too small")) };
        // if (lastList.withdrawalCnt >= WithdrawalCnt) { return (alert("Withdrawal Count too small")) };
        api.post('/api/Marketing/AddPointLevelInfo', newBankList)
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    props.ClickSave(newBankList);
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
                    <div className="popup__head"><span className="text">Add new point level</span>
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
                                                    <span className="text">Point</span>
                                                </div>
                                                <div className="textfield">
                                                    <input readOnly defaultValue={pointLevel} onChange={(e) => { setPoint(parseInt(e.target.value)) }} />
                                                </div>
                                            </div>
                                            <div className="form__group">
                                                <div className="form__title">
                                                    <span className="text">Deposit Count</span>
                                                </div>
                                                <div className="textfield">
                                                    <input type="number" defaultValue={depositCnt} onChange={(e) => { setDepositCnt(parseInt(e.target.value)) }} />
                                                </div>
                                            </div>
                                            <div className="form__group">
                                                <div className="form__title">
                                                    <span className="text">Withdrawal Count</span>
                                                </div>
                                                <div className="textfield">
                                                    <input type="number" defaultValue={withdrawalCnt} onChange={(e) => { setWithdrawalCnt(parseInt(e.target.value)) }} />
                                                </div>
                                            </div>
                                            <div className="form__group">
                                                <div className="form__title">
                                                    <span className="text">IsKyc</span>
                                                </div>
                                                <div className="textfield">
                                                    <input type="checkbox" checked={(IsKyc === 1)} onChange={(e) => { setIsKyc((e.target.checked) ? 1 : 0) }} />
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