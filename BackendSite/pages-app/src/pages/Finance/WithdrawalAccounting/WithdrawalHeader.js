import React from "react";

const Panel = (props) => {
  var accountingAmount = 0;
  for (var i = 0; i < props.TransIdData.AccountingDatas.length; i++) {
    accountingAmount += props.TransIdData.AccountingDatas[i].amount;
  }

  var accountingChargeFee = 0;
  for (var i = 0; i < props.TransIdData.AccountingDatas.length; i++) {
    accountingChargeFee += props.TransIdData.AccountingDatas[i].chargeFee;
  }

  return (
    <div className="filter">
      <h5>Withdrawal Info</h5>
      <table className="table table-detail">
        <tbody>
          <tr>
            <th scope="row">TransId</th>
            <td>{props.TransIdData.Withdrawal.transId}</td>
            <th scope="row">SiteName</th>
            <td>{props.TransIdData.Withdrawal.siteName}</td>
            <th scope="row">Currency</th>
            <td>{props.TransIdData.IsoCode}</td>
          </tr>
          <tr>
            <th scope="row">UserName</th>
            <td>
              <a
                href={
                  "/CustManage/CustInfo?UserName=" +
                  props.TransIdData.Withdrawal.userName +
                  "&custId=" +
                  props.TransIdData.Withdrawal.custId +
                  "&TransId=" +
                  props.TransIdData.Withdrawal.transId
                }
                target="_blank"
              >
                {props.TransIdData.Withdrawal.userName}
              </a>
            </td>
            <th scope="row">WithdrawalType</th>
            <td>LocalBank</td>
            <th scope="row">FromBankAccountNo</th>
            <td>
              {props.TransIdData.Withdrawal.fromBankCode}
              {"-"}
              {props.TransIdData.Withdrawal.fromAccountName}
              {"-"}
              {props.TransIdData.Withdrawal.fromAccountNo}
            </td>
          </tr>
        </tbody>
      </table>
      <h5>
        Accounting Summary{" "}
        <span style={{ color: "red", fontWeight: "bold" }}>
          {"     Total Count: "}
          {props.TransIdData.AccountingDatas.length}
        </span>
      </h5>
      <table className="table table-detail">
        <tbody>
          <tr>
            <th scope="row"></th>
            <th scope="row">Withdrawal Info</th>
            <th scope="row">Accounting Summary</th>
            <th scope="row">Compare Result</th>
            <th scope="row" style={{ width: "150px" }}>
              Description
            </th>
          </tr>
          <tr>
            <th scope="row">Amount</th>
            <td>{props.TransIdData.Withdrawal.amount}</td>
            <td>{accountingAmount}</td>
            {CompareResult(
              accountingAmount,
              props.TransIdData.Withdrawal.amount
            )}
            {CompareDescription(
              "Amount",
              accountingAmount,
              props.TransIdData.Withdrawal.amount
            )}
          </tr>
          <tr>
            <th scope="row">ChargeFee</th>
            <td> {props.TransIdData.Withdrawal.chargeFee}</td>
            <td> {accountingChargeFee}</td>
            {CompareResult(
              accountingChargeFee,
              props.TransIdData.Withdrawal.chargeFee
            )}
            {CompareDescription(
              "ChargeFee",
              accountingChargeFee,
              props.TransIdData.Withdrawal.chargeFee
            )}
          </tr>
        </tbody>
      </table>
      <div>
        <button
          className="btn btn-back"
          type="back"
          onClick={props.GoBack}
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
          onClick={props.Create.bind(this, props.TransIdData)}
          style={{
            marginTop: "20px",
            marginBottom: "40px",
            marginRight: "10px",
          }}
        >
          Create
        </button>
      </div>
    </div>
  );
};

const CompareResult = (accountingNum, withdrawalNum) => {
  if (accountingNum > withdrawalNum) {
    return <td style={{ fontWeight: "bold", background: "red" }}>Over</td>;
  } else if (accountingNum < withdrawalNum) {
    return <td style={{ fontWeight: "bold", background: "yellow" }}>Under</td>;
  } else {
    return <td style={{ fontWeight: "bold", background: "#04AA6D" }}>Equal</td>;
  }
};

const CompareDescription = (text, accountingNum, withdrawalNum) => {
  if (accountingNum > withdrawalNum) {
    return <td>accounting num is MORE THAN withdrawal num.</td>;
  } else if (accountingNum < withdrawalNum) {
    return <td>accounting num is LESS THAN withdrawal num.</td>;
  } else {
    return <td>accounting num is EQUAL withdrawal num.</td>;
  }
};

export default Panel;
