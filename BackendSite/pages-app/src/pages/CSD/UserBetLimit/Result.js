import React from "react";
import Pagination from "./Pagination";
import StatusButton from "./Components/StatusButton";
import ActionButtons from "./Components/ActionButton";
import moment from "moment-timezone";

const SpecialItemType = {
	actionBtn: "actionBtn",
  statusBtn: "statusBtn"
};

const dataColumnDefs = () => {
  return [
    { dataColumn: "siteName", columnTitle: "SiteName" },
    {
      dataColumn: "userName",
      columnTitle: "UserName",
      body: {
        format: userNameFormat,
        subData: ["custId"],
      },
    },
    { dataColumn: "creator", columnTitle: "Creator"},
    {
      dataColumn: "createTime",
      columnTitle: "CreateTime (GMT+8)",
      body: {
        format: dateTimeFormat,
      },
    },
    { dataColumn: "updateOperator", columnTitle: "LastUpdateUser"},
    {
      dataColumn: "updateTime",
      columnTitle: "UpdateTime (GMT+8)",
      body: {
        format: dateTimeFormat,
      },
    },
    {
        dataColumn: "status",
        columnTitle: "Status",
        body: { 
          SpecialItem: SpecialItemType.statusBtn 
        }
    },
    {
        columnTitle: "Action",
        header: {
            attr: {
                className: "width-85"
            }
        },
        body: {
            SpecialItem: SpecialItemType.actionBtn
        }
    }
  ];
};

export default function Result (userBetLimit) {
    const columnDefs = dataColumnDefs();
    let headerRows = [];

    columnDefs.forEach((element, idx) => {
        headerRows.push(
        <th
            key={`th-${element.dataColumn}-${idx}`}
            scope="col"
            className={
            element.header && element.header.attr && element.header.attr.class
            ? element.header.attr.class
            : ""}>
            {element.columnTitle}
        </th>);
    });
    let bodyRows = userBetLimit.Datas.map((data, i) => {
        let row = columnDefs.map((element, j) => {
            
            let body_content = null;
            if (element.body && element.body.format) {
                let subData = {};
                if(element.body.subData) {
                    element.body.subData.forEach((s) => {
                    subData[s] = data[s];
                    });
                }
                body_content = element.body.format(data[element.dataColumn], subData)

            }
            else if (element.body && element.body.SpecialItem && element.body.SpecialItem === SpecialItemType.actionBtn) {
                body_content = <ActionButtons 
                                  ClickEdit={userBetLimit.ClickEdit.bind(this, data["custId"])}
                                  ClickDelete={userBetLimit.ClickDelete.bind(this, data["custId"])}/>

            }
            else if(element.body && element.body.SpecialItem && element.body.SpecialItem === SpecialItemType.statusBtn) {
                body_content =  <StatusButton 
                                  mainKey={data["custId"]} 
                                  checked={data["status"] === 1} 
                                  onChange={userBetLimit.UpdateStatus.bind(this, data["custId"], data["status"])}
                                />
            }
            else {
                body_content = data[element.dataColumn]
            }

            return (
                <td
                key={`td-${element.dataColumn}-${j}`}
                className={
                    element.body && element.body.attr && element.body.attr.class
                    ? element.body.attr.class
                    : ""
                }>
                {body_content}
                </td>
            );
        });

        return <tr key={`result-body-${i}`}>{row}</tr>;
    });

  return (
    <div className="table">
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
      <Pagination
        Data={{
          totalPages: userBetLimit.PageInfo.totalPages,
          pageNumber: userBetLimit.PageInfo.pageNumber,
          rowCount: userBetLimit.PageInfo.rowCount,
        }}
        ChangePage={userBetLimit.ChangePage}
      />
    </div>
  );
};

const userNameFormat = (userName, subData) => {
  return (
    <a
      href={
        "/CustManage/CustInfo?UserName=" +
        userName +
        "&custId=" +
        subData.custId +
        "&TransId="
      }
      target="_blank"
    >
      {userName}
    </a>
  );
};

const dateTimeFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};