import { number_format } from "../../commJS/common";
import moment from "moment-timezone";
import Pagination from "./Pagination";

const BonusInfoStatus = {
  1: <span className="status status-active">Running</span>,
  2: <span className="status status-suspend">Canceled</span>,
  3: <span className="status status-closed">Expired</span>,
  4: <span className="status status-initial">Reached</span>,
};

const Result = (props) => {
  let rows =
    props.Datas.length === 0
      ? null
      : props.Datas.map((d, dIdx) => {
          return (
            <tr key={`result-${dIdx}`}>
              <td>{d.bonusId}</td>
              <td>{d.bonusCode}</td>
              <td>
                {d.description}-
                {d.atPage == 0 ? "Promotion page" : "Deposit page"}
              </td>
              <td>{number_format(d.bonusAmount)}</td>
              <td>{number_format(d.lockedAmount)}</td>
              <td>{number_format(d.custRollOver.rollOver)}</td>
              <td>{ d.fullFillType == 2 ? number_format(d.custRollOver.winOver) : 0}</td>
              <td>{number_format(d.custRollOver.turnOver)}</td>
              <td>{number_format(d.cancelBonusAmount)}</td>
              <td>{moment(d.claimDate).format("l LTS")}</td>
              <td>
                {BonusInfoStatus[d.bonusInfoStatus]}
                {d.bonusInfoStatus == 1 ? "" : "-"}
                {d.bonusInfoStatus == 1
                  ? ""
                  : moment(d.updateTime).format("l LTS")}
              </td>
              <td>{d.expiredCancel ? "TRUE" : "FALSE"}</td>
              <td>
                {d.bonusInfoStatus == 1 ? (
                  <button
                    className="btn btn-primary"
                    onClick={props.CancelBonus.bind(this, d.bonusId)}
                  >
                    Cancel Bonus
                  </button>
                ) : null}
              </td>
            </tr>
          );
        });
  return (
    <>
      <div className="table-responsive-md table">
        <span class="label">Promotion</span>
        <table className="table table-striped">
          <thead>
            <tr>
              <th scope="col">Bonus Id</th>
              <th scope="col">BonusCode</th>
              <th scope="col">Description</th>
              <th scope="col">Bonus Amount</th>
              <th scope="col">Locked Amount</th>
              <th scope="col">Rollover/Winover</th>
              <th scope="col">Winover</th>
              <th scope="col">Turnover</th>
              <th scope="col">Cancel Bonus Amount</th>
              <th scope="col">ClaimDate (Adjusted to GMT+8)</th>
              <th scope="col">Status</th>
              <th scope="col">Expired Cancel</th>
              <th scope="col"></th>
            </tr>
          </thead>
          {rows}
        </table>
        <Pagination
          Data={{
            totalPages: props.PageInfo.totalPages,
            pageNumber: props.PageInfo.pageNumber,
            rowCount: props.PageInfo.rowCount,
            currencyAmount: props.PageInfo.currencyAmount,
          }}
          ChangePage={props.ChangePage}
        />
      </div>
    </>
  );
};

export default Result;
