import React from 'react';
import { number_format } from '../../../module/Formatting/NumberFormat';
import { RadioBtn } from './component/RadioBox';
import { InputField } from './component/InputBox';
import { TextArea } from './component/Textarea';
import { SelectList } from './component/SelectList';

export const CustInfo = (props) => {
    let optionRadio = Object.keys(props.AdjustmentOptionData).map((key) => {
        return (
            <RadioBtn key={`type-${key}`} Id={`type-${key}`} Value={key} DisplayText={props.AdjustmentOptionData[key]} OnChange={(e) => { props.SendFiled('adjustmentOpt', key) }} Disabled={props.StateSend} Checked={props.SendData.adjustmentOpt === key} />
        )
    });
    return (
        props.CustData != null ?
            <div>
                <h4 className="page-name">Cust Info</h4>
                <table className="table table-detail">
                    <tbody>
                        <tr>
                            <th scope="row">Site Name</th>
                            <td>{props.CustData.siteName}</td>
                        </tr>
                        <tr>
                            <th scope="row">User Name</th>
                            <td>{props.CustData.userName}</td>
                        </tr>
                        <tr>
                            <th scope="row">Wallet Amount</th>
                            <td>{number_format(props.CustData.balance)}</td>
                        </tr>
                    </tbody>
                </table>
                <div className="form-group">
                    <label className="col-form-label d-block">Option</label>
                    {optionRadio}
                </div>
                <InputField
                    IptType="text"
                    columnTitle="Amount"
                    defaultValue={props.SendData.amount}
                    value={props.SendData.amount}
                    OnChange={props.SendFiled.bind(null, 'amount')}
                    OnPressEnter={props.OnPressEnter}
                    Disabled={props.StateSend}
                />
                <SelectList
                    DisplayText="Adjust Reason"
                    value={props.SendData.category}
                    OnChange={props.SendFiled.bind(null, 'category')}
                    Disabled={props.StateSend}
                    SelectOptions={props.AdjustCategory}
                />
                <TextArea DisplayText="Remarks" OnBlur={props.SendFiled.bind(null, 'memo')} OnPressEnter={props.OnPressEnter} Disabled={props.StateSend} defaultValue={props.SendData.memo} value={props.SendData.memo} />
                <div className="button-group mt-10">
                    <button type="submit" className="btn btn-primary" onClick={props.OnPressEnter}>Submit</button>
                </div>
            </div> : null
    )
}