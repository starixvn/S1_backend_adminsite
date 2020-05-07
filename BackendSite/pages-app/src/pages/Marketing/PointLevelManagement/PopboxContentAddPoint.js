import React, { useState } from "react";
import {api} from '../../../module/WebAPI';
export const PopBoxContentAddPoint = (props) => {
    let lastList = props.BankList.length === 0 ? [] : props.BankList[props.BankList.length - 1];
    let siteId = props.SiteId, currencyId = props.CurrencyId, pointLevel = 1, depositLeast = 1, betLeast = 1;

    if (lastList.length !== 0) {
        pointLevel = parseInt(lastList.pointLevel) + 1;
        depositLeast = parseInt(lastList.depositLeast) + 1;
        betLeast = parseInt(lastList.betLeast) + 1;
    }

    const [Point, setPoint] = useState(pointLevel);
    const [DepositLeast, setDepositLeast] = useState(depositLeast);
    const [BetLeast, setBetLeast] = useState(betLeast);
    let newBankList = { siteId: siteId, currencyId: currencyId, pointLevel: Point, depositLeast: DepositLeast, betLeast: BetLeast }
    const handleSave = (e) => {
        e.preventDefault();

        if (lastList.pointLevel >= Point) { return (alert("err1")) };
        if (lastList.depositLeast >= DepositLeast) { return (alert("Deposit least too small")) };
        if (lastList.betLeast >= BetLeast) { return (alert("Bet least too small")) };
        api.post('/api/Marketing/AddPointLevelInfo', { siteId: siteId, currencyId: currencyId, pointLevel: Point, depositLeast: DepositLeast, betLeast: BetLeast })
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
                        <label>Deposit least</label>
                        <input type="number" defaultValue={depositLeast} onChange={(e) => { setDepositLeast(parseInt(e.target.value)) }} />
                    </div>
                    <div className="field">
                        <label>Bet least</label>
                        <input type="number" defaultValue={betLeast} onChange={(e) => { setBetLeast(parseInt(e.target.value)) }} />
                    </div>
                    <button className="ui right floated primary button" onClick={handleSave} >Save</button>
                    <button className="ui right floated button" onClick={props.ClickCancel}>Cancel</button>
                    <div className="clearfix"></div>
                </form>
            </div>
        </div>
    )
}