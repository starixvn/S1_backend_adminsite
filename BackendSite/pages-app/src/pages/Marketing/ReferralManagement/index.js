import React, { useState, useEffect, useRef } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import qs from "qs";

const App = () => {
  const [siteList, setsiteList] = useState([]);
  const [site, setsite] = useState(1);
  const [referral, setreferral] = useState([]);
  const [focusid, setfocusid] = useState("");
  const [mode, setmode] = useState(0);
  let headerRows = [];
  useEffect(() => {
    let param = {
      Url: "BonusManage/GetSiteListByLoginUser",
    };

    api.post("/api/BonusManage/GetSiteListByLoginUser").then((res) => {
      if (res.data.errorCode === 0) {
        let result = [];
        res.data.data.map((e, index) => {
          result.push({ value: e.siteId, displayText: e.siteName });
        });
        setsiteList(result);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);
  const titleData = [
    { dataColumn: "Currency", columnTitle: "Currency" },
    { dataColumn: "Validity", columnTitle: "Validity" },
    { dataColumn: "VaildBet", columnTitle: "VaildBet" },
    { dataColumn: "Deposit", columnTitle: "Deposit" },
    { dataColumn: "BonusAmount", columnTitle: "BonusAmount" },
    { dataColumn: "Status", columnTitle: "Status" },
  ];
  titleData.forEach((e, i) => {
    headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
  });
  const siteChange = (value) => {
    setsite(value);
  };

  const SelectBox = ({
    Options,
    HandleChange,
    SelectedValue,
    dataColumn,
    Disabled,
    Title,
    iptSubClass,
  }) => {
    const OnChange = (e) => {
      HandleChange(e.target.value, dataColumn);
    };
    return (
      <div className="input-item">
        {Title ? <label className="label">{Title}</label> : null}
        <div className={`field${iptSubClass ? " " + iptSubClass : ""}`}>
          <select
            className="field"
            value={SelectedValue}
            onChange={OnChange}
            disabled={Disabled}
          >
            {Options &&
              Options.map((o) => {
                return (
                  <option key={o.value} value={o.value}>
                    {o.displayText}
                  </option>
                );
              })}
          </select>
        </div>
      </div>
    );
  };

  const InputBox = (props) => {
    const OnChange = (e) => {
      let value = e.target.value;
      let checked =
        value === "" || props.regex === undefined
          ? true
          : value.match(props.regex);
      if (checked) {
        props.OnChange(value, props.dataColumn);
      }
    };
    return (
      <div className={props.ClassName ? props.ClassName : "input-item"}>
        <div
          className={`field${props.iptSubClass ? " " + props.iptSubClass : ""}`}
        >
          {props.SubTitle ? (
            <span className="bule-bold">{props.SubTitle}</span>
          ) : null}

          <input
            id={`${props.dataColumn}${props.Index}`}
            type={props.type ? props.type : "text"}
            value={
              props.value === null || props.value === undefined
                ? ""
                : props.value
            }
            onChange={OnChange}
            disabled={props.Disabled === true}
            className={props.inputClass ? props.inputClass : ""}
          />
        </div>
      </div>
    );
  };

  const InputBoxHidden = (props) => {
    const OnChange = (e) => {
      let value = e.target.value;
      let checked =
        value === "" || props.regex === undefined
          ? true
          : value.match(props.regex);
      if (checked) {
        props.OnChange(value, props.dataColumn);
      }
    };
    return (
      <div className={props.ClassName ? props.ClassName : "input-item"}>
        <div
          className={`field${props.iptSubClass ? " " + props.iptSubClass : ""}`}
        >
          {props.SubTitle ? (
            <span className="bule-bold">{props.SubTitle}</span>
          ) : null}
          <input
            id={`${props.dataColumn}${props.Index}`}
            type={props.type ? props.type : "text"}
            value={
              props.value === null || props.value === undefined
                ? ""
                : props.value
            }
            onChange={OnChange}
            disabled={props.Disabled === true}
            className={props.inputClass ? props.inputClass : ""}
            type="hidden"
          />
        </div>
      </div>
    );
  };

  const Search = () => {
    let formData = new FormData();
    formData.append("SiteId", parseInt(site));
    api
      .post("/api/BonusManage/GetReferral", formData)
      .then(function (response) {
        console.log(response.data.data);
        setreferral(response.data.data);
        // bodyRows();
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Submit = () => {
    let check = true;
    if (
      check &&
      window.confirm("Are you want to change Referral Manegement?")
    ) {
      let formData = new FormData();
      formData.append("referral", JSON.stringify(referral));
      api
        .post("/api/BonusManage/SetReferral", formData)
        .then(function (response) {
          if (response.data.errorCode === 999) {
            alert(response.data.message);
          } else {
            setmode(0);
            alert("存檔成功");
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };
  const Edit = (outindex, value, name) => {
    let result = referral;
    result[outindex][name] = value;
    setreferral([...result]);
    setfocusid(name + outindex);
  };

  useEffect(() => {
    let ele = document.getElementById(focusid);
    if (ele) {
      ele.focus();
    }
  }, [referral]);

  const handleToggle = (outindex, value, name, site, currencyId) => {
    if (window.confirm("Are you sure change Status ?")) {
      let formData = new FormData();
      formData.append("SiteId", parseInt(site));
      formData.append("CurrencyId", parseInt(currencyId));
      api
        .post("/api/BonusManage/SetReferralStatus", formData)
        .then(function (response) {
          // bodyRows();
          if (response.data.errorCode === 7) {
            alert("請先新增該筆的資料，才允許調整狀態");
          } else {
            let result = referral;
            result[outindex][name] = !value;
            setreferral([...result]);
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };

  const Modify = () => {
    setmode(1);
  };

  const View = () => { 
    let formData = new FormData();
    formData.append("SiteId", parseInt(site));
    api
      .post("/api/BonusManage/GetReferral", formData)
      .then(function (response) {
        setreferral(response.data.data);
      })
      .catch(function (error) {
        alert(error);
      });
    setmode(0);
  };

  const data = referral.map((item, index) => {
    return (
      <tr key={index}>
        <td>
          <InputBoxHidden
            Title="SiteId"
            OnChange={Edit.bind(this, index)}
            dataColumn="siteId"
            value={item.siteId}
            Disabled={true}
            Index={index}
          />
          <InputBoxHidden
            Title="CurrencyId"
            OnChange={Edit.bind(this, index)}
            dataColumn="currencyId"
            value={item.currencyId}
            Disabled={true}
            Index={index}
          />
          <span className="label">{item.isoCode}</span>
        </td>
        <td>
          {mode === 1 ? (
            <InputBox
              Title="PeriodDays"
              OnChange={Edit.bind(this, index)}
              dataColumn="periodDays"
              value={item.periodDays}
              Disabled={mode !== 1}
              Index={index}
              type="number"
            />
          ) : (
            <span className="label">{item.periodDays}</span>
          )}
        </td>
        <td>
          {mode === 1 ? (
            <InputBox
              Title="VaildBet"
              OnChange={Edit.bind(this, index)}
              dataColumn="vaildBet"
              value={item.vaildBet === "" ? "" : parseFloat(item.vaildBet)}
              Disabled={mode !== 1}
              Index={index}
              type="number"
            />
          ) : (
            <span className="label">
              {item.vaildBet === "" ? "" : parseFloat(item.vaildBet)}
            </span>
          )}
        </td>
        <td>
          {mode === 1 ? (
            <InputBox
              Title="Deposit"
              OnChange={Edit.bind(this, index)}
              dataColumn="deposit"
              value={item.deposit === "" ? "" : parseFloat(item.deposit)}
              Disabled={mode !== 1}
              Index={index}
              type="number"
            />
          ) : (
            <span className="label">
              {item.deposit === "" ? "" : parseFloat(item.deposit)}
            </span>
          )}
        </td>
        <td>
          {mode === 1 ? (
            <InputBox
              Title="BonusAmount"
              OnChange={Edit.bind(this, index)}
              dataColumn="bonusAmount"
              value={
                item.bonusAmount === "" ? "" : parseFloat(item.bonusAmount)
              }
              Disabled={mode !== 1}
              Index={index}
              type="number"
            />
          ) : (
            <span className="label">
              {item.bonusAmount === "" ? "" : parseFloat(item.bonusAmount)}
            </span>
          )}
        </td>
        <td>
          {item.status ? "Yes" : "No"}
          {mode !== 1 ? (
            <div className="switch__container">
              <input
                id={`switch-${index}`}
                className="switch switch--flat"
                checked={item.status}
                type="checkbox"
                onChange={() =>
                  handleToggle(
                    index,
                    item.status,
                    "status",
                    item.siteId,
                    item.currencyId
                  )
                }
                disabled={mode !== 0}
              />
              <label htmlFor={`switch-${index}`}></label>
            </div>
          ) : null}
        </td>
      </tr>
    );
  });
  return (
    <>
      <div className="container-fluid">
        <h4 className="page-name">Referral Management</h4>
      </div>
      <div>
        <div className="filter">
          <div className="row-flex">
            <SelectBox
              Title="Site"
              Options={siteList}
              HandleChange={siteChange}
              dataColumn="site"
              SelectedValue={site}
              Disabled={false}
            />
          </div>
          <div>
            <button
              className="btn btn-add"
              style={{
                marginTop: "10px",
                marginBottom: "10px"
              }}
              onClick={Search}
            >
              Search
            </button>
          </div>
        </div>
      </div>
      <div className="table">
        {referral.length !== 0 ? (
          <table className="table table-bordered">
            <thead>
              <tr>{headerRows}</tr>
            </thead>
            <tbody>{data}</tbody>
          </table>
        ) : null}
      </div>
      <div>
        {referral.length !== 0 ? (
          mode !== 1 ? null : (
            <button
              className="btn btn-add"
              style={{
                marginTop: "20px",
                marginBottom: "40px",
                marginRight: "10px",
              }}
              onClick={Submit}
            >
              Save
            </button>
          )
        ) : null}
        {referral.length !== 0 ? (
          mode !== 1 ? (
            <button
              className="btn btn-add"
              style={{
                marginTop: "20px",
                marginBottom: "40px",
                marginRight: "10px",
              }}
              onClick={Modify}
            >
              Edit
            </button>
          ) : (
            <button
              className="btn btn-add"
              style={{
                marginTop: "20px",
                marginBottom: "40px",
                marginRight: "10px",
              }}
              onClick={View}
            >
              Cancel
            </button>
          )
        ) : null}
      </div>
    </>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
