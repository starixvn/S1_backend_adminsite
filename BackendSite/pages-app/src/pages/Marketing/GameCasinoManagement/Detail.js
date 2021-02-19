import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";

const Panel = (props) => {
  const titleData = [
    { dataColumn: "Hot", columnTitle: "Hot" },
    { dataColumn: "Featured", columnTitle: "Featured" },
    { dataColumn: "Status", columnTitle: "Status" },
  ];
  let headerRows = [];
  titleData.forEach((e, i) => {
    headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
  });
  const handleToggle = (type, value) => {
    if (window.confirm(`Are you sure change ${type} ?`)) {
      let Data = {
        SiteId: props.Datas.SiteId,
        CurrencyId: props.Datas.CurrencyId,
        GameCasinoId: props.Datas.GameCasinoId,
        Hot: props.Datas.Hot,
        Featured: props.Datas.Featured,
        Status: props.Datas.Status,
      };
      if (type === "Hot") {
        props.SetdetailDatas({
          ...props.Datas,
          Hot: !value,
        });
        Data.Hot = !Data.Hot;
      }
      if (type === "Featured") {
        props.SetdetailDatas({
          ...props.Datas,
          Featured: !value,
        });
        Data.Featured = !Data.Featured;
      }
      if (type === "Status") {
        props.SetdetailDatas({
          ...props.Datas,
          Status: !value,
        });
        Data.Status = !Data.Status;
      }
      console.log(props);
      api
        .post("/api/GameCasinoManagement/SetGameCasinoDetail", Data)
        .then(function (response) {})
        .catch(function (error) {
          alert(error);
        });
    }
  };
  return (
    <div className="table">
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>
          {props.Datas.SiteId === 0 ? null : (
            <tr key={0}>
              <td>
                {props.Datas.Hot ? "Yes" : "No"}
                <div className="switch__container">
                  <input
                    id={`switch-${0}`}
                    className="switch switch--flat"
                    checked={props.Datas.Hot}
                    type="checkbox"
                    onChange={() => handleToggle("Hot", props.Datas.Hot)}
                    disabled={false}
                  />
                  <label htmlFor={`switch-${0}`}></label>
                </div>
              </td>
              <td>
                {props.Datas.Featured ? "Yes" : "No"}
                <div className="switch__container">
                  <input
                    id={`switch-${1}`}
                    className="switch switch--flat"
                    checked={props.Datas.Featured}
                    type="checkbox"
                    onChange={() =>
                      handleToggle("Featured", props.Datas.Featured)
                    }
                    disabled={false}
                  />
                  <label htmlFor={`switch-${1}`}></label>
                </div>
              </td>
              <td>
                {props.Datas.Status ? "Yes" : "No"}
                <div className="switch__container">
                  <input
                    id={`switch-${2}`}
                    className="switch switch--flat"
                    checked={props.Datas.Status}
                    type="checkbox"
                    onChange={() => handleToggle("Status", props.Datas.Status)}
                    disabled={false}
                  />
                  <label htmlFor={`switch-${2}`}></label>
                </div>
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
};

export default Panel;
