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
        <div className="popup">
            <div className="popup_inner">
                <h1>Add new point level</h1>
                <form className="ui form">
                    <div className="field">
                        <label>Point</label>
                        <input readOnly defaultValue={pointLevel} onChange={(e) => { setPoint(parseInt(e.target.value)) }} />
                    </div>
                    <div className="field">
                        <label>Deposit Count</label>
                        <input type="number" defaultValue={depositCnt} onChange={(e) => { setDepositCnt(parseInt(e.target.value)) }} />
                    </div>
                    <div className="field">
                        <label>Withdrawal Count</label>
                        <input type="number" defaultValue={withdrawalCnt} onChange={(e) => { setWithdrawalCnt(parseInt(e.target.value)) }} />
                    </div>
                    <div className="field">
                        <label>IsKyc</label>
                        <input type="checkbox" checked={(IsKyc === 1)} onChange={(e) => { setIsKyc((e.target.checked) ? 1 : 0) }} />
                    </div>
                    <button className="ui right floated primary button" onClick={handleSave} >Save</button>
                    <button className="ui right floated button" onClick={props.ClickCancel}>Cancel</button>
                    <div className="clearfix"></div>
                </form>
            </div>
        </div>
    )
}