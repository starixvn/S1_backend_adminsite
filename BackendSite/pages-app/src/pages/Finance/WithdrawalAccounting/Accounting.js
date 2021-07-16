import React, { useState } from "react";
import moment from "moment";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const elemType = {
  DateTimePicker: "DateTimePicker",
};
const dateformat = "YYYY-MM-DD HH:mm";

const EditPanel = (props) => {
  var accountingAmount = 0;
  for (var i = 0; i < props.ForCaculateDatas.AccountingDatas.length; i++) {
    accountingAmount += props.ForCaculateDatas.AccountingDatas[i].amount;
  }

  var accountingChargeFee = 0;
  for (var i = 0; i < props.ForCaculateDatas.AccountingDatas.length; i++) {
    accountingChargeFee += props.ForCaculateDatas.AccountingDatas[i].chargeFee;
  }

  let nowAccountingAmount = accountingAmount - props.AccountingData.amount;
  let nowAccountingChargeFee =
    accountingChargeFee - props.AccountingData.chargeFee;

  const [stateData, updateData] = useState({
    id: props.AccountingData.id,
    transId: props.AccountingData.transId,
    seq: props.AccountingData.seq,
    bankAccountNo: props.AccountingData.bankAccountNo,
    amount: props.AccountingData.amount,
    chargeFee: props.AccountingData.chargeFee,
    memo: props.AccountingData.memo,
    creator: props.AccountingData.creator,
    creatorName: props.AccountingData.creatorName,
    createTime: props.AccountingData.createTime,
    operator: props.AccountingData.operator,
    operatorName: props.AccountingData.operatorName,
    operateTime: props.AccountingData.operateTime,
  });

  const EditFiled = (v, datacolumn) => {
    updateData({
      ...stateData,
      [datacolumn]: v,
    });
  };

  const EditBankAccountNoFiled = (v) => {
    console.log(v);
    updateData({
      ...stateData,
      bankAccountNo: v,
    });
  };

  return (
    <>
      <div>
        <button
          className="btn btn-back"
          type="back"
          onClick={props.GoBack.bind(this, stateData.transId)}
          style={{
            marginTop: "20px",
            marginBottom: "40px",
            marginRight: "10px",
          }}
        >
          Back
        </button>
        <button
          className="btn btn-add"
          onClick={props.Submit.bind(this, stateData)}
          style={{
            marginTop: "20px",
            marginBottom: "40px",
            marginRight: "10px",
          }}
        >
          Submit
        </button>
      </div>
      <InputField
        columnTitle="TransId"
        Value={stateData.transId}
        dataColumn="transId"
        defaultValue=""
        Size={500}
        Disabled={true}
      />
      <InputField
        columnTitle="Seq"
        Value={stateData.seq}
        dataColumn="seq"
        defaultValue=""
        Size={500}
        Disabled={true}
      />
      <DropDownBox
        columnTitle="Bank Account No."
        options={GetBankAccountOptions(props.BankList)}
        value={stateData.bankAccountNo}
        dataColumn="bankAccountNo"
        onChange={EditFiled}
      />
      <CompareInputField
        columnTitle="Amount"
        Value={stateData.amount}
        dataColumn="amount"
        defaultValue="0"
        Size={500}
        type="number"
        onChange={EditFiled}
        AlreadyNum={nowAccountingAmount}
        WithdrawalNum={props.ForCaculateDatas.Withdrawal.amount}
        Disabled={false}
      />
      <CompareInputField
        columnTitle="ChargeFee"
        Value={stateData.chargeFee}
        dataColumn="chargeFee"
        defaultValue="0"
        Size={500}
        type="number"
        onChange={EditFiled}
        Disabled={false}
        AlreadyNum={nowAccountingChargeFee}
        WithdrawalNum={props.ForCaculateDatas.Withdrawal.chargeFee}
      />
      <InputField
        columnTitle="Memo"
        Value={stateData.memo}
        dataColumn="memo"
        defaultValue=""
        Size={800}
        onChange={EditFiled}
        Disabled={false}
      />
      <InputField
        columnTitle="Creator"
        Value={stateData.creatorName}
        dataColumn="creatorName"
        defaultValue=""
        Size={500}
        Disabled={true}
      />
      <InputField
        columnTitle="Create Time"
        Value={dateFormat(stateData.createTime)}
        dataColumn="createTime"
        defaultValue=""
        Size={500}
        Disabled={true}
      />
      <InputField
        columnTitle="Operator"
        Value={stateData.operatorName}
        dataColumn="operatorName"
        defaultValue=""
        Size={500}
        Disabled={true}
      />
      <InputField
        columnTitle="Operate Time"
        Value={dateFormat(stateData.operateTime)}
        dataColumn="operateTime"
        defaultValue=""
        Size={500}
        Disabled={true}
      />
      <br></br>
    </>
  );
};

const GetBankAccountOptions = (list) => {
  let Options = [];
  Options.push({ value: "", displayText: " - Select -" });
  if (list != null) {
    list.forEach((x) => {
      Options.push({
        value: x.accountNo,
        displayText: x.bankCode + "-" + x.accountName + "-" + x.accountNo,
      });
    });
  }
  return Options;
};

const DropDownBox = (props) => {
  const EditField = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    } else if (props.onBlur) {
      props.onBlur(e.target.value, props.dataColumn);
    }
  };
  let optionRows = props.options.map((item) => {
    return (
      <option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>
        {item.displayText}
      </option>
    );
  });
  return (
    <div>
      <div className="input-item" style={{ width: "500px", zIndex: "1" }}>
        <span className="label">{props.columnTitle}</span>
        <div className="field">
          <select
            className="form-control"
            onChange={EditField}
            value={props.value}
          >
            {optionRows}
          </select>
        </div>
      </div>
    </div>
  );
};

const InputField = (props) => {
  const EditField = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    } else if (props.onBlur) {
      props.onBlur(e.target.value, props.dataColumn);
    }
  };
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <input
        className="form-control"
        value={props.Value}
        onChange={EditField}
        onBlur={EditField}
        disabled={props.Disabled}
        type={props.type ? props.type : "text"}
        style={{ width: `${props.Size}px` }}
      />
    </div>
  );
};

const CompareInputField = (props) => {
  const EditField = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    } else if (props.onBlur) {
      props.onBlur(e.target.value, props.dataColumn);
    }
  };
  return (
    <div className="row" style={{ marginLeft: "auto" }}>
      <div>
        <span className="label">This Accounting {props.columnTitle}</span>
        <input
          className="form-control"
          value={props.Value}
          onChange={EditField}
          onBlur={EditField}
          disabled={props.Disabled}
          type={props.type ? props.type : "text"}
          style={{ width: `${props.Size}px` }}
        />
      </div>
      <h5 className="label" style={{ marginTop: "auto" }}>
        +
      </h5>
      <div>
        <span className="label"> Already Accounting {props.columnTitle}</span>
        <input
          className="form-control"
          value={numberFormat(props.AlreadyNum)}
          disabled={true}
          //type={props.type ? props.type : "text"}
          style={{ width: "200px" }}
        />
      </div>
      {CompareMessage(
        Number(props.Value),
        props.AlreadyNum,
        props.WithdrawalNum
      )}
      <div>
        <span className="label"> withdrawal {props.columnTitle}</span>
        <input
          className="form-control"
          value={numberFormat(props.WithdrawalNum)}
          disabled={true}
          //type={props.type ? props.type : "text"}
          style={{ width: "200px" }}
        />
      </div>
    </div>
  );
};

const CompareMessage = (nowNum, alreadyNum, withdrawalNum) => {
  console.log(nowNum, alreadyNum, withdrawalNum);
  if (nowNum + alreadyNum > withdrawalNum) {
    return (
      <h5
        className="label"
        style={{
          marginTop: "auto",
          width: "100px",
          textAlign: "center",
          color: "red",
        }}
      >
        More Than
      </h5>
    );
  } else if (nowNum + alreadyNum < withdrawalNum) {
    return (
      <h5
        className="label"
        style={{
          marginTop: "auto",
          width: "100px",
          textAlign: "center",
          color: "red",
        }}
      >
        Less Than
      </h5>
    );
  } else {
    return (
      <h5
        className="label"
        style={{
          marginTop: "auto",
          width: "100px",
          textAlign: "center",
          color: "#04AA6D",
        }}
      >
        Equal
      </h5>
    );
  }
};

const numberFormat = (number) => {
  return number_format(number_toFixed(number, 0));
};

const dateFormat = (datetime) => {
  return moment(datetime).format("l LTS");
};

export default EditPanel;
