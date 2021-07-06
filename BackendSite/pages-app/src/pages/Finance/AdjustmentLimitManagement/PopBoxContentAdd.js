import React, { useState } from "react";
import { api } from '../../../module/WebAPI';

export const PopBoxContentAdd = (props) => {
    const [AdjustmentMax, setAdjustmentMax] = useState(0);

    const handleSave = (e) => {
        e.preventDefault();
        api.post('/api/Payment/AddAdjustmentLimitInfo', { SiteId: props.SiteId, CurrencyId: props.CurrencyId,AdjustmentMax:parseFloat(AdjustmentMax)})
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    alert(response.data.message);
                    props.ClickCancel();
                    props.ReGetData();
                } else {
                    alert(response.data.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }
    return (
        <div>
            <div>
                <h1>Add Adjustment Limit</h1>
                <form className="ui form">
                    <div className="field">
                        <label>Adjustment Max</label>
                        <input type="number" defaultValue={AdjustmentMax} onChange={(e) => { setAdjustmentMax(parseFloat(e.target.value)) }} />
                        <label>(0 = Unlimited)</label>
                    </div>
                    <button className="ui right floated primary button" onClick={handleSave} >Save</button>
                    <button className="ui right floated button" onClick={props.ClickCancel}>Cancel</button>
                    <div className="clearfix"></div>
                </form>
            </div>
        </div>
    )
}